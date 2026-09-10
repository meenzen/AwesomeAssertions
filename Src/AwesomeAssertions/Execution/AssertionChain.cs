using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using AwesomeAssertions.Common;
using AwesomeAssertions.Formatting;

namespace AwesomeAssertions.Execution;

/// <summary>
/// Provides a fluent API to build simple or composite assertions, and which can flow from one assertion to another.
/// </summary>
/// <remarks>
/// This is the core engine of many of the assertion APIs in this library. When combined with <see cref="AssertionScope"/>,
/// you can run multiple assertions which failure messages will be collected until the scope is disposed.
/// </remarks>
public sealed class AssertionChain
{
    private readonly Func<AssertionScope> getCurrentScope;

    /// <summary>
    /// Determines the caller identifier the way this chain was set up to do it.
    /// </summary>
    /// <remarks>
    /// This is infrastructure of the chain and deliberately not part of <see cref="State"/>. It is composed once per
    /// chain and must survive resetting the state for a nested assertion, whereas everything in <see cref="State"/>
    /// describes a single assertion and must not leak into that nested assertion. The per-assertion counterpart is
    /// <see cref="State.CallerIdentifierOverride"/>, which takes precedence as long as it is set.
    /// </remarks>
    private readonly Func<string> getDefaultCallerIdentifier;

    private State state = new();

    private static readonly AsyncLocal<AssertionChain> Instance = new();

    /// <summary>
    /// The effective caller identifier including any prefixes and postfixes configured through
    /// <see cref="WithCallerPostfix"/>.
    /// </summary>
    /// <remarks>
    /// Can be overridden with <see cref="OverrideCallerIdentifier"/>.
    /// </remarks>
    public string CallerIdentifier =>
        (state.CallerIdentifierOverride ?? getDefaultCallerIdentifier)() + state.CallerPostfix;

    /// <summary>
    /// Exposes the options which will be used for formatting objects in case an assertion fails.
    /// </summary>
    internal FormattingOptions FormattingOptions => getCurrentScope().FormattingOptions;

    /// <summary>
    /// Indicates whether the caller identifier has been manually overridden.
    /// </summary>
    /// <remarks>
    /// This property is used to track if the caller identifier has been customized using the
    /// <see cref="OverrideCallerIdentifier"/> method or similar methods that modify the identifier.
    /// </remarks>
    public bool HasOverriddenCallerIdentifier =>
        state.CallerIdentifierOverride is not null || state.CallerPostfix.Length > 0;

    /// <summary>
    /// Gets a value indicating whether all assertions in the <see cref="AssertionChain"/> have succeeded.
    /// </summary>
    public bool Succeeded => state.PreviousAssertionSucceeded && state.Succeeded is null or true;

    /// <summary>
    /// Forces the objects involved in the assertion to be formatted using line breaks in the failure message.
    /// </summary>
    public AssertionChain UsingLineBreaks
    {
        get
        {
            getCurrentScope().FormattingOptions.UseLineBreaks = true;
            return this;
        }
    }

    /// <summary>
    /// Either starts a new assertion chain, or, when <see cref="ReuseOnce"/> was called, for once, will return
    /// an existing instance.
    /// </summary>
    public static AssertionChain GetOrCreate()
    {
        if (Instance.Value != null)
        {
            AssertionChain assertionChain = Instance.Value;
            Instance.Value = null;
            return assertionChain;
        }

        return new AssertionChain(() => AssertionScope.Current,
            () => AwesomeAssertions.CallerIdentifier.DetermineCallerIdentity());
    }

    /// <summary>
    /// Ensures that the next call to <see cref="GetOrCreate"/> will reuse the current instance.
    /// </summary>
    public void ReuseOnce()
    {
        Instance.Value = this;
    }

    private AssertionChain(Func<AssertionScope> getCurrentScope, Func<string> getCallerIdentifier)
    {
        this.getCurrentScope = getCurrentScope;

        getDefaultCallerIdentifier = () =>
        {
            var scopeName = getCurrentScope().Name();
            var callerIdentifier = getCallerIdentifier();

            if (scopeName is null)
            {
                return callerIdentifier;
            }
            else if (callerIdentifier is null)
            {
                return scopeName;
            }
            else
            {
                return $"{scopeName}/{callerIdentifier}";
            }
        };
    }

    /// <summary>
    /// Adds an explanation of why the assertion is supposed to succeed to the scope.
    /// </summary>
    /// <param name="reason">
    /// An object containing a formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed, as well as zero or more objects to format the placeholders.
    /// If the phrase does not start with the word <i>because</i>, it is prepended automatically.explaining why the assertion is needed.
    /// </param>
    public AssertionChain BecauseOf(Reason reason)
    {
        return BecauseOf(reason.FormattedMessage, reason.Arguments);
    }

    /// <summary>
    /// Adds an explanation of why the assertion is supposed to succeed to the scope.
    /// </summary>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AssertionChain BecauseOf(string because, params object[] becauseArgs)
    {
        state.Reason = () =>
        {
            try
            {
                string becauseOrEmpty = because ?? string.Empty;

                return becauseArgs?.Length > 0
                    ? string.Format(CultureInfo.InvariantCulture, becauseOrEmpty, becauseArgs)
                    : becauseOrEmpty;
            }
            catch (FormatException formatException)
            {
                return
                    $"**WARNING** because message '{because}' could not be formatted with string.Format{Environment.NewLine}{formatException.StackTrace}";
            }
        };

        return this;
    }

    /// <summary>
    /// Specifies the condition that must be satisfied for the assertion to succeed.
    /// </summary>
    /// <param name="condition">
    /// If <see langword="true"/> the assertion is treated as successful; if <see langword="false"/> the next call
    /// to one of the <c>FailWith</c> overloads records the failure.
    /// </param>
    /// <remarks>
    /// The condition is ignored when a prior assertion in the chain already failed.
    /// </remarks>
    public AssertionChain ForCondition(bool condition)
    {
        if (state.PreviousAssertionSucceeded)
        {
            state.Succeeded = condition;
        }

        return this;
    }

    /// <summary>
    /// Specifies the condition that must be satisfied for the assertion to succeed.
    /// </summary>
    /// <param name="condition">
    /// A function returning <see langword="true"/> when the assertion should be treated as successful, or
    /// <see langword="false"/> to have the next call to one of the <c>FailWith</c> overloads record the failure.
    /// </param>
    /// <remarks>
    /// The <paramref name="condition"/> is only invoked when a prior assertion in the chain succeeded.
    /// </remarks>
    public AssertionChain ForCondition(Func<bool> condition)
    {
        if (state.PreviousAssertionSucceeded)
        {
            state.Succeeded = condition();
        }

        return this;
    }

    /// <summary>
    /// Specifies that the occurrence <paramref name="constraint"/> must satisfy the number <paramref name="actualOccurrences"/>
    /// for the assertion to succeed.
    /// </summary>
    /// <param name="constraint">
    /// The occurrence constraint (such as those produced by <c>AtLeast</c>, <c>AtMost</c> or <c>Exactly</c>) to evaluate.
    /// </param>
    /// <param name="actualOccurrences">The number of occurrences that were actually found.</param>
    /// <remarks>
    /// The constraint is not evaluated when a prior assertion in the chain already failed.
    /// </remarks>
    public AssertionChain ForConstraint(OccurrenceConstraint constraint, int actualOccurrences)
    {
        if (state.PreviousAssertionSucceeded)
        {
            constraint.RegisterContextData((key, value) => state.ContextData.Add(
                new ContextDataDictionary.DataItem(key, value, reportable: false, requiresFormatting: false)));

            state.Succeeded = constraint.Assert(actualOccurrences);
        }

        return this;
    }

    /// <summary>
    /// Specifies that the assertion executed by <paramref name="failingAssertion"/> must fail for the current
    /// assertion to succeed.
    /// </summary>
    /// <typeparam name="TAssertions">
    /// The assertion class that <paramref name="failingAssertion"/> returns a constraint for. It is inferred from
    /// that return value and never needs to be specified.
    /// </typeparam>
    /// <param name="failingAssertion">
    /// The assertion that is expected to fail. This is typically the positive counterpart of the assertion being
    /// implemented, such as <c>() => BeEquivalentTo(unexpected)</c> within <c>NotBeEquivalentTo</c>.
    /// </param>
    /// <remarks>
    /// The assertion is not executed when a prior assertion in the chain already failed. Any failures it records are
    /// discarded, because they describe the opposite expectation and would be misleading.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="failingAssertion"/> is <see langword="null"/>.</exception>
    public AssertionChain ForFailingAssertion<TAssertions>(Func<AndConstraint<TAssertions>> failingAssertion)
    {
        Guard.ThrowIfArgumentIsNull(failingAssertion);

        if (state.PreviousAssertionSucceeded)
        {
            // The nested assertion changes the current chain's state, so we have to restore it afterwards.
            State savedState = state;
            var savedInstance = Instance.Value;

            // and it must not see anything of the assertion that is being built here
            state = new State();
            Instance.Value = null;

            using AssertionScope scope = new();
            failingAssertion();

            bool hasFailures = scope.HasFailures();

            // we don't want to see the failures, because here they indicate that we were successful.
            _ = scope.Discard();

            // restore the state after the nested assertion
            state = savedState;
            Instance.Value = savedInstance;

            // and update the success state
            state.Succeeded = hasFailures;
        }

        return this;
    }

    /// <summary>
    /// Defines the expectation part of the failure message that is prepended to the failure reason of the assertions
    /// executed within <paramref name="chain"/>.
    /// </summary>
    /// <param name="message">
    /// The expectation shown as part of the failure message. May contain numbered
    /// <see cref="string.Format(string,object[])"/>-style placeholders as well as specialized placeholders.
    /// </param>
    /// <param name="arg1">An object to format using the placeholders in <paramref name="message"/>.</param>
    /// <param name="chain">The assertions to execute using the specified expectation.</param>
    public Continuation WithExpectation(string message, object arg1, Action<AssertionChain> chain)
    {
        return WithExpectation(message, chain, arg1);
    }

    /// <summary>
    /// Defines the expectation part of the failure message that is prepended to the failure reason of the assertions
    /// executed within <paramref name="chain"/>.
    /// </summary>
    /// <param name="message">
    /// The expectation shown as part of the failure message. May contain numbered
    /// <see cref="string.Format(string,object[])"/>-style placeholders as well as specialized placeholders.
    /// </param>
    /// <param name="arg1">The first object to format using the placeholders in <paramref name="message"/>.</param>
    /// <param name="arg2">The second object to format using the placeholders in <paramref name="message"/>.</param>
    /// <param name="chain">The assertions to execute using the specified expectation.</param>
    public Continuation WithExpectation(string message, object arg1, object arg2, Action<AssertionChain> chain)
    {
        return WithExpectation(message, chain, arg1, arg2);
    }

    /// <summary>
    /// Defines the expectation part of the failure message that is prepended to the failure reason of the assertions
    /// executed within <paramref name="chain"/>.
    /// </summary>
    /// <param name="message">
    /// The expectation shown as part of the failure message. May contain specialized placeholders such as
    /// <em>{context}</em>.
    /// </param>
    /// <param name="chain">The assertions to execute using the specified expectation.</param>
    public Continuation WithExpectation(string message, Action<AssertionChain> chain)
    {
        return WithExpectation(message, chain, []);
    }

    private Continuation WithExpectation(string message, Action<AssertionChain> chain, params object[] args)
    {
        if (state.PreviousAssertionSucceeded)
        {
            state.Expectation = () =>
            {
                var formatter = new FailureMessageFormatter(getCurrentScope().FormattingOptions)
                    .WithReason(state.Reason?.Invoke() ?? string.Empty)
                    .WithContext(state.ContextData)
                    .WithIdentifier(CallerIdentifier)
                    .WithFallbackIdentifier(state.FallbackIdentifier);

                return formatter.Format(message, args);
            };

            chain(this);

            state.Expectation = null;
        }

        return new Continuation(this);
    }

    /// <summary>
    /// Sets the identifier that is used in the failure message when the caller identifier could not be determined
    /// and no other identifier was provided.
    /// </summary>
    /// <param name="identifier">The fallback identifier to use in the failure message.</param>
    public AssertionChain WithDefaultIdentifier(string identifier)
    {
        state.FallbackIdentifier = identifier;
        return this;
    }

    /// <summary>
    /// Allows executing an assertion against the object returned by <paramref name="selector"/>, which is only invoked
    /// when the previous assertion in the chain succeeded.
    /// </summary>
    /// <param name="selector">A function that returns the object on which the continued assertion is executed.</param>
    /// <returns>
    /// A <see cref="GivenSelector{T}"/> that can be used to continue the assertion on the selected object.
    /// </returns>
    public GivenSelector<T> Given<T>(Func<T> selector)
    {
        return new GivenSelector<T>(selector, this);
    }

    [StackTraceHidden]
    internal Continuation FailWithPreFormatted(string formattedFailReason)
    {
        return FailWith(() => formattedFailReason);
    }

    /// <summary>
    /// Records a failure with the specified <paramref name="message"/> when the condition set through one of the
    /// <see cref="ForCondition(bool)"/> overloads was not met.
    /// </summary>
    /// <param name="message">
    /// The failure message. May contain specialized placeholders such as <em>{reason}</em> and <em>{context}</em>.
    /// </param>
    [StackTraceHidden]
    public Continuation FailWith(string message)
    {
        return FailWith(() => new FailReason(message));
    }

    /// <summary>
    /// Records a failure with the specified <paramref name="message"/> when the condition set through one of the
    /// <see cref="ForCondition(bool)"/> overloads was not met.
    /// </summary>
    /// <param name="message">
    /// The failure message. May contain numbered <see cref="string.Format(string,object[])"/>-style placeholders as well
    /// as specialized placeholders.
    /// </param>
    /// <param name="args">
    /// Zero or more objects to format using the placeholders in <paramref name="message"/>.
    /// </param>
    [StackTraceHidden]
    public Continuation FailWith(string message, params object[] args)
    {
        return FailWith(() => new FailReason(message, args));
    }

    /// <summary>
    /// Records a failure with the specified <paramref name="message"/> when the condition set through one of the
    /// <see cref="ForCondition(bool)"/> overloads was not met.
    /// </summary>
    /// <param name="message">
    /// The failure message. May contain numbered <see cref="string.Format(string,object[])"/>-style placeholders as well
    /// as specialized placeholders.
    /// </param>
    /// <param name="argProviders">
    /// Zero or more functions that provide the objects to format using the placeholders in <paramref name="message"/>.
    /// Each function is only invoked when the assertion actually failed.
    /// </param>
    [StackTraceHidden]
    public Continuation FailWith(string message, params Func<object>[] argProviders)
    {
        return FailWith(() => new FailReason(
            message,
            argProviders.Select(a => a()).ToArray()));
    }

    /// <summary>
    /// Records a failure using the <see cref="FailReason"/> produced by <paramref name="getFailureReason"/> when the
    /// condition set through one of the <see cref="ForCondition(bool)"/> overloads was not met.
    /// </summary>
    /// <param name="getFailureReason">
    /// A function that produces the <see cref="FailReason"/> describing the failure. It is only invoked when the
    /// assertion actually failed.
    /// </param>
    [StackTraceHidden]
    public Continuation FailWith(Func<FailReason> getFailureReason)
    {
        return FailWith(() =>
        {
            var formatter = new FailureMessageFormatter(getCurrentScope().FormattingOptions)
                .WithReason(state.Reason?.Invoke() ?? string.Empty)
                .WithContext(state.ContextData)
                .WithIdentifier(CallerIdentifier)
                .WithFallbackIdentifier(state.FallbackIdentifier);

            FailReason failReason = getFailureReason();

            return formatter.Format(failReason.Message, failReason.Args);
        });
    }

    [StackTraceHidden]
    private Continuation FailWith(Func<string> getFailureReason)
    {
        if (state.PreviousAssertionSucceeded)
        {
            state.PreviousAssertionSucceeded = state.Succeeded is true;

            if (state.Succeeded is not true)
            {
                string failure = getFailureReason();

                if (state.Expectation is not null)
                {
                    failure = state.Expectation() + failure;
                }

                getCurrentScope().AddPreFormattedFailure(failure.Capitalize().RemoveTrailingWhitespaceFromLines());
            }
        }

        // Reset the state for successive assertions on this object
        state.Succeeded = null;

        return new Continuation(this);
    }

    /// <summary>
    /// Allows overriding the caller identifier for the next call to one of the `FailWith` overloads instead
    /// of relying on the automatic behavior that extracts the variable names from the C# code.
    /// </summary>
    public void OverrideCallerIdentifier(Func<string> getCallerIdentifier)
    {
        state.CallerIdentifierOverride = getCallerIdentifier;
    }

    /// <summary>
    /// Adds a postfix such as <c>[0]</c> to the caller identifier detected by the library.
    /// </summary>
    /// <remarks>
    /// Can be used by an assertion that uses <see cref="AndWhichConstraint{TParent,TSubject}"/> to return an object or
    /// collection on which another assertion is executed, and which wants to amend the automatically detected caller
    /// identifier with a postfix.
    /// </remarks>
    public AssertionChain WithCallerPostfix(string postfix)
    {
        state.CallerPostfix = postfix;

        return this;
    }

    /// <summary>
    /// <para>
    /// Obsolete, use <see cref="WithReportable(string,string)"/> instead.
    /// </para>
    /// <para>
    /// Adds named information to the assertion chain, which will be included
    /// in the message emitted if the chain finally fails.
    /// </para>
    /// </summary>
    /// <param name="key">The key of the information to add.</param>
    /// <param name="value">The value of the information to add.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AddReportable(string key, string value)
    {
        getCurrentScope().AddReportable(key, value);
    }

    /// <summary>
    /// <para>
    /// Obsolete, use <see cref="WithReportable(string,Func{string})"/> instead.
    /// </para>
    /// <para>
    /// Adds named information to the assertion chain, which will be included
    /// in the message emitted if the chain finally fails. The value is only calculated on failure.
    /// </para>
    /// </summary>
    /// <param name="key">The key of the information to add.</param>
    /// <param name="valueFunc">Calculates the value of the information to add upon failure.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AddReportable(string key, Func<string> valueFunc)
    {
        getCurrentScope().AddReportable(key, valueFunc);
    }

    /// <summary>
    /// Adds named information to the assertion chain, which will be included
    /// in the message emitted if the chain finally fails.
    /// </summary>
    /// <param name="key">The name of the information to add.</param>
    /// <param name="value">The value of the information to add.</param>
    public AssertionChain WithReportable(string key, string value)
    {
        getCurrentScope().AddReportable(key, value);
        return this;
    }

    /// <summary>
    /// Adds named information to the assertion chain, which will be included
    /// in the message emitted if the chain finally fails.
    /// </summary>
    /// <param name="key">The key of the information to add.</param>
    /// <param name="valueFunc">Calculates the value of the information to add upon failure.</param>
    public AssertionChain WithReportable(string key, Func<string> valueFunc)
    {
        getCurrentScope().AddReportable(key, valueFunc);
        return this;
    }

    /// <summary>
    /// Adds named information to the assertion chain, which will be included
    /// in the message emitted if the chain finally fails.
    /// </summary>
    /// <param name="key">The key of the information to add.</param>
    /// <param name="value">The value of the information to add. Is formatted using the <see cref="Formatter"/> upon failure.</param>
    public AssertionChain WithReportable(string key, object value)
    {
        getCurrentScope().AddReportable(key, value);
        return this;
    }

    /// <summary>
    /// Registers a failure in the chain that doesn't need any parsing or formatting anymore.
    /// </summary>
    [StackTraceHidden]
    internal void AddPreFormattedFailure(string failure)
    {
        getCurrentScope().AddPreFormattedFailure(failure);
    }

    private sealed class State
    {
        public ContextDataDictionary ContextData { get; } = new();

        public string FallbackIdentifier { get; set; } = "object";

        // The caller identification set through <see cref="OverrideCallerIdentifier"/>,
        // or <see langword="null"/> when the chain determines the caller itself.
        public Func<string> CallerIdentifierOverride { get; set; }

        public Func<string> Reason { get; set; }

        public bool? Succeeded { get; set; }

        // The expectation that WithExpectation installs for the duration of its callback. FailWith prepends it to the
        // failure reason, so that several FailWith calls can share one expectation. Null means there is none.
        public Func<string> Expectation { get; set; }

        public string CallerPostfix { get; set; } = string.Empty;

        // Unlike Succeeded, which FailWith resets after every single assertion, this latches: once an assertion in the
        // chain failed, it stays false, so a later successful assertion cannot make the chain look successful again.
        public bool PreviousAssertionSucceeded { get; set; } = true;
    }
}
