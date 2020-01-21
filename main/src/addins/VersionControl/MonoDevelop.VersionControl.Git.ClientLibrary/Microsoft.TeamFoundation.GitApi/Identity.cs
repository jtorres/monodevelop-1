//*************************************************************************************************
// Identity.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using Microsoft.TeamFoundation.GitApi.Internal;
using Newtonsoft.Json;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Object-model representing a Git identity.
    /// </summary>
    public interface IIdentity : IEquatable<IIdentity>
    {
        /// <summary>
        /// Gets the email address associated with this identity.
        /// </summary>
        string Email { get; }

        /// <summary>
        /// Gets the time stamp associated with this identity.
        /// </summary>
        DateTimeOffset Timestamp { get; }

        /// <summary>
        /// Gets the username associated with this identity.
        /// </summary>
        string Username { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class Identity : Base, IEquatable<Identity>, IIdentity, ILoggable
    {
        public const string DefaultEmail = "Unknown";
        public static DateTimeOffset DefaultTimestamp = DateTimeOffset.MaxValue;
        public const string DefaultUsername = "Unknown";
        public static readonly StringComparer EmailComparer = StringComparer.InvariantCultureIgnoreCase;
        public static readonly StringComparer UsernameComparer = StringComparer.InvariantCultureIgnoreCase;

        private static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        private Identity()
            : base()
        {
        }

        private StringUtf8 _email;
        private DateTimeOffset _timestamp;
        private StringUtf8 _username;

        internal void SetContextAndCache(IExecutionContext context, IStringCache cache)
        {
            SetContext(context);
            _cache = cache;

            if (cache != null)
            {
                _username = cache.Intern(_username);
                _email = cache.Intern(_email);
            }
        }

        [JsonProperty]
        public string Email
        {
            get
            {
                string email = (string)_email;

                if (_cache != null)
                {
                    email = _cache.Intern(email);
                }

                return email;
            }
        }

        public StringUtf8 EmailUtf8
        {
            get { return _email; }
        }

        [JsonProperty]
        public string Username
        {
            get
            {
                string username = (string)_username;

                if (_cache != null)
                {
                    username = _cache.Intern(username);
                }

                return username;
            }
        }

        public StringUtf8 UsernameUtf8
        {
            get { return _username; }
        }

        [JsonProperty]
        public DateTimeOffset Timestamp
        {
            get { return _timestamp; }
        }

        private IStringCache _cache;

        public static Identity Create(StringUtf8 username, StringUtf8 email, DateTimeOffset timestamp)
        {
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (email == null)
                throw new ArgumentNullException(nameof(email));

            return new Identity()
            {
                _email = email,
                _username = username,
                _timestamp = timestamp,
            };
        }

        public static Identity Create(StringUtf8 username, StringUtf8 email)
            => Create(username, email, DateTimeOffset.Now);

        public static bool Equals(IIdentity left, IIdentity right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(null, right))
                return false;

            return UsernameComparer.Equals(left.Username, right.Username)
                && EmailComparer.Equals(left.Email, right.Email);
        }

        public override bool Equals(object obj)
            => Identity.Equals(this as IIdentity, obj as IIdentity);

        public bool Equals(Identity other)
            => Identity.Equals(this as IIdentity, other as IIdentity);

        public bool Equals(IIdentity other)
            => Identity.Equals(this as IIdentity, other);

        public override int GetHashCode()
        {
            int h1 = UsernameComparer.GetHashCode(_username);
            int h2 = EmailComparer.GetHashCode(_email);

            unchecked
            {
                return (h1 << 5) ^ h2
                     + (h2 << 5) ^ h1;
            }
        }

        public override string ToString()
        {
            return $"{_username} <{_email}> {_timestamp}";
        }

        internal static Identity FromUtf8(ByteBuffer buffer, int start, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(start + count) > buffer.Length)
                throw new IndexOutOfRangeException(nameof(count));

            try
            {
                int firstChevron = buffer.FirstIndexOf('<', start, count);
                if (firstChevron < 0)
                    throw new ObjectParseException("identity<", new StringUtf8(buffer, start, count), 0);

                int finalChevron = buffer.FirstIndexOf('>', firstChevron, start + count - firstChevron);
                if (finalChevron < 0)
                    throw new ObjectParseException("identity>", new StringUtf8(buffer, start, count), 0);

                // find the time zone marker, and treat it as the end of the string
                // we're recording all times in GMT - is this right, do not ask me.
                int tzMarker = buffer.LastIndexOf('+', finalChevron, start + count - finalChevron);
                if (tzMarker < 0)
                {
                    tzMarker = buffer.LastIndexOf('-', finalChevron, start + count - finalChevron);
                }

                // no time zone offset is possible, account for it
                if (tzMarker < 0)
                {
                    tzMarker = count + 1;
                }

                var username = new StringUtf8(buffer, start, firstChevron - start - 1);
                var email = new StringUtf8(buffer, firstChevron + 1, finalChevron - firstChevron - 1);
                var time = new StringUtf8(buffer, finalChevron + 2, tzMarker - finalChevron - 3);

                DateTimeOffset timestamp = DateTimeOffset.MinValue;
                long seconds = 0;
                if (time.TryParse(out seconds))
                {
                    timestamp = FromUnixTimeSeconds(seconds);
                }

                return Create(username, email, timestamp);
            }
            catch (Exception exception)
            {
                throw new ObjectParseException("identity-exception", new StringUtf8(buffer, start, count), 0, exception);
            }
        }

        private static DateTimeOffset FromUnixTimeSeconds(long seconds)
        {
            return UnixEpoch.AddSeconds(seconds);
        }

        void ILoggable.Log(ExecutionContext context, System.Text.StringBuilder log, int indent)
        {
            string prefix = context.ParseHelper.GetParseErrorIndent(indent);
            log.Append(prefix).AppendLine($"{nameof(Identity)} {{ user-name: \"{Username}\", email: \"{Email}\", time-stamp: {Timestamp.ToString()} }}");
        }
    }
}
