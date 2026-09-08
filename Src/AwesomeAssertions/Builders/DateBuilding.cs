using System.Diagnostics.Contracts;

namespace AwesomeAssertions.Builders;

/// <summary>
/// Provides extension methods for creating date builders.
/// </summary>
public static class DateBuilding
{
    extension(int day)
    {
        /// <summary>
        /// Creates a new <see cref="DateBuilder"/> for January of the specified year.
        /// </summary>
        /// <param name="year">
        /// The year for which to create the <see cref="DateBuilder"/>.
        /// </param>
        [Pure]
        public DateBuilder Jan(int year) => new(year, 01, day);

        /// <summary>
        /// Creates a new <see cref="DateBuilder"/> for February of the specified year.
        /// </summary>
        /// <param name="year">
        /// The year for which to create the <see cref="DateBuilder"/>.
        /// </param>
        [Pure]
        public DateBuilder Feb(int year) => new(year, 02, day);

        /// <summary>
        /// Creates a new <see cref="DateBuilder"/> for March of the specified year.
        /// </summary>
        /// <param name="year">
        /// The year for which to create the <see cref="DateBuilder"/>.
        /// </param>
        [Pure]
        public DateBuilder Mar(int year) => new(year, 03, day);

        /// <summary>
        /// Creates a new <see cref="DateBuilder"/> for April of the specified year.
        /// </summary>
        /// <param name="year">
        /// The year for which to create the <see cref="DateBuilder"/>.
        /// </param>
        [Pure]
        public DateBuilder Apr(int year) => new(year, 04, day);

        /// <summary>
        /// Creates a new <see cref="DateBuilder"/> for May of the specified year.
        /// </summary>
        /// <param name="year">
        /// The year for which to create the <see cref="DateBuilder"/>.
        /// </param>
        [Pure]
        public DateBuilder May(int year) => new(year, 05, day);

        /// <summary>
        /// Creates a new <see cref="DateBuilder"/> for June of the specified year.
        /// </summary>
        /// <param name="year">
        /// The year for which to create the <see cref="DateBuilder"/>.
        /// </param>
        [Pure]
        public DateBuilder Jun(int year) => new(year, 06, day);

        /// <summary>
        /// Creates a new <see cref="DateBuilder"/> for July of the specified year.
        /// </summary>
        /// <param name="year">
        /// The year for which to create the <see cref="DateBuilder"/>.
        /// </param>
        [Pure]
        public DateBuilder Jul(int year) => new(year, 07, day);

        /// <summary>
        /// Creates a new <see cref="DateBuilder"/> for August of the specified year.
        /// </summary>
        /// <param name="year">
        /// The year for which to create the <see cref="DateBuilder"/>.
        /// </param>
        [Pure]
        public DateBuilder Aug(int year) => new(year, 08, day);

        /// <summary>
        /// Creates a new <see cref="DateBuilder"/> for September of the specified year.
        /// </summary>
        /// <param name="year">
        /// The year for which to create the <see cref="DateBuilder"/>.
        /// </param>
        [Pure]
        public DateBuilder Sep(int year) => new(year, 09, day);

        /// <summary>
        /// Creates a new <see cref="DateBuilder"/> for October of the specified year.
        /// </summary>
        /// <param name="year">
        /// The year for which to create the <see cref="DateBuilder"/>.
        /// </param>
        [Pure]
        public DateBuilder Oct(int year) => new(year, 10, day);

        /// <summary>
        /// Creates a new <see cref="DateBuilder"/> for November of the specified year.
        /// </summary>
        /// <param name="year">
        /// The year for which to create the <see cref="DateBuilder"/>.
        /// </param>
        [Pure]
        public DateBuilder Nov(int year) => new(year, 11, day);

        /// <summary>
        /// Creates a new <see cref="DateBuilder"/> for December of the specified year.
        /// </summary>
        /// <param name="year">
        /// The year for which to create the <see cref="DateBuilder"/>.
        /// </param>
        [Pure]
        public DateBuilder Dec(int year) => new(year, 12, day);
    }
}
