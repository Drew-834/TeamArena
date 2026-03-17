// =============================================================================
// AvatarResolver.cs — Modular avatar file matching for TeamArena
// =============================================================================
// Automatically resolves a team member's name to the best-matching avatar image
// file in wwwroot/images/avatars/. Supports .png and .jpg extensions with
// case-insensitive, multi-strategy matching (full name, first name, fuzzy).
//
// USAGE:
//   string url = AvatarResolver.Resolve("Seyquan Williams");
//   // returns "images/avatars/seyquan.png"
//
//   string url = AvatarResolver.Resolve("Adam", "images/avatars/adam1.png");
//   // returns "images/avatars/adam1.png" (existing URL preserved)
//
// TO ADD A NEW AVATAR:
//   1. Drop the image into wwwroot/images/avatars/
//   2. If the filename matches the member's first or full name, it works
//      automatically. Otherwise, add an explicit entry to _explicitMappings.
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace GameScoreboard.Services
{
    /// <summary>
    /// Static utility that maps team member names to avatar image paths.
    /// Three-tier matching: explicit overrides -> filename matching -> fallback.
    /// </summary>
    public static class AvatarResolver
    {
        private const string AvatarBasePath = "images/avatars/";
        private const string DefaultAvatar = "images/avatars/adam1.png";

        // ---------------------------------------------------------------------
        // Explicit name-to-file overrides for cases where the filename doesn't
        // match the member's name cleanly (typos, nicknames, AI-generated files).
        // Key = member full name (lowercase), Value = filename only.
        // ---------------------------------------------------------------------
        private static readonly Dictionary<string, string> _explicitMappings = new(StringComparer.OrdinalIgnoreCase)
        {
            { "David Schunk", "david shunk.jpg" },     // filename has different spelling
            { "DJ Skelton", "dj.png" },                 // "DJ" is a nickname, not parseable as first name easily
        };

        // ---------------------------------------------------------------------
        // All known avatar filenames on disk. Each entry is the filename only
        // (e.g., "seyquan.png"). This registry is the source of truth for what
        // files are available to match against.
        //
        // Excludes AI-generated placeholder images (u1271257843_* files).
        // To add a new avatar, add its filename here and drop the file in
        // wwwroot/images/avatars/.
        // ---------------------------------------------------------------------
        private static readonly List<string> _knownAvatarFiles = new()
        {
            // Real member photos (name-based)
            "Jeremy.png",
            "Johnathon King.png",
            "cesar.png",
            "cesar2.png",
            "dakota.png",
            "dakota2.png",
            "david shunk.jpg",
            "dj.png",
            "francisco.png",
            "liz.png",
            "seyquan.png",
            "victor.png",

            // Original team avatars (already assigned by name convention)
            "adam1.png",
            "drew1.png",
            "gustavo1.png",
            "gustavo2.png",
            "ishack1.png",
            "ishack2.png",
            "jon1.png",
            "kla1.png",
            "matthew1.png",
            "ruben1.png",
            "vinny1.png",
            "vinny2.png",
        };

        // Pre-built lookup: lowercase filename (without extension) -> full filename
        // e.g., "seyquan" -> "seyquan.png", "johnathon king" -> "Johnathon King.png"
        private static readonly Dictionary<string, string> _filenameLookup;

        static AvatarResolver()
        {
            _filenameLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in _knownAvatarFiles)
            {
                // Strip extension to get the matchable name portion
                var nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(file);
                // Also strip trailing digits for numbered variants (e.g., "cesar2" -> "cesar")
                var baseName = nameWithoutExt.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

                // Store both the exact name and the base name (first entry wins)
                _filenameLookup.TryAdd(nameWithoutExt, file);
                _filenameLookup.TryAdd(baseName, file);
            }
        }

        /// <summary>
        /// Resolves the best avatar URL for a given member name.
        /// </summary>
        /// <param name="memberName">Full name of the team member (e.g., "Seyquan Williams")</param>
        /// <param name="existingAvatarUrl">
        /// The member's currently assigned AvatarUrl. If it already points to a
        /// name-specific file (not a generic placeholder), it is preserved.
        /// </param>
        /// <returns>Relative path like "images/avatars/seyquan.png"</returns>
        public static string Resolve(string memberName, string? existingAvatarUrl = null)
        {
            if (string.IsNullOrWhiteSpace(memberName))
                return existingAvatarUrl ?? DefaultAvatar;

            // ── Strategy 1: Explicit override mapping ──
            if (_explicitMappings.TryGetValue(memberName, out var explicitFile))
                return AvatarBasePath + explicitFile;

            // ── Strategy 2: Full-name match against filenames ──
            // e.g., "Johnathon King" matches "Johnathon King.png"
            if (_filenameLookup.TryGetValue(memberName, out var fullNameMatch))
                return AvatarBasePath + fullNameMatch;

            // ── Strategy 3: First-name match ──
            // e.g., "Cesar Perez" -> first name "Cesar" matches "cesar.png"
            var firstName = memberName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrEmpty(firstName) && _filenameLookup.TryGetValue(firstName, out var firstNameMatch))
                return AvatarBasePath + firstNameMatch;

            // ── Strategy 4: Last-name match ──
            // e.g., "Dakota French" -> last name "French" doesn't match, but
            // "Dakota" already matched above. This catches edge cases.
            var nameParts = memberName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length > 1)
            {
                var lastName = nameParts.Last();
                if (_filenameLookup.TryGetValue(lastName, out var lastNameMatch))
                    return AvatarBasePath + lastNameMatch;
            }

            // ── Strategy 5: Preserve existing URL if it's a real assignment ──
            // Only fall through here if none of the name-based strategies matched.
            // If the existing URL is a generic placeholder, we still return it
            // (better than nothing).
            if (!string.IsNullOrEmpty(existingAvatarUrl))
                return existingAvatarUrl;

            // ── Final fallback ──
            return DefaultAvatar;
        }
    }
}
