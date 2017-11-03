using System;

namespace Newtonsoft.Json.Utilities
{
    internal class EnumBidirectionalDictionary
    {
        private bool _hasCaseSensitiveMembers = false;
        private readonly BidirectionalDictionary<string, string> _caseInsensitiveDictionary = 
            new BidirectionalDictionary<string, string>(StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);

        private BidirectionalDictionary<string, string> _caseSensitiveDictionary;

        private void InitilizeCaseSensitiveDictionary()
        {
            _caseSensitiveDictionary = new BidirectionalDictionary<string, string>(StringComparer.Ordinal, StringComparer.Ordinal);
        }

        public void Set(string first, string second)
        {
            // Add to caseSensitive if second is found, since second is used by TryResolvedEnumName in EnumUtils
            if (_caseInsensitiveDictionary.TryGetBySecond(second, out string _))
            {
                if (!_hasCaseSensitiveMembers)
                {
                    _hasCaseSensitiveMembers = true;

                    // Initilize case sensitive dictionary
                    InitilizeCaseSensitiveDictionary();
                }

                // Add new element
                _caseSensitiveDictionary.Set(first, second);
                
                // Not needed to add the current element from caseInsentive to caseSensitive since we lookup in _caseInsensitive after _caseSensitive

                // Return so we don't also add it to the case insensitive dictionary
                return;
            }

            // Add to case insensitive dictionary
            _caseInsensitiveDictionary.Set(first, second);
        }

        public bool TryGetByFirst(string first, out string second)
        {
            if (_hasCaseSensitiveMembers)
            {
                // Try match case sentitive dictionary and if match - return
                if (_caseSensitiveDictionary.TryGetByFirst(first, out second))
                {
                    return true;
                }
            }

            // Match on case insensitive
            return _caseInsensitiveDictionary.TryGetByFirst(first, out second);
        }

        public bool TryGetBySecond(string second, out string first)
        {
            if (_hasCaseSensitiveMembers)
            {
                // Try match case sentitive dictionary and if match - return
                if (_caseSensitiveDictionary.TryGetBySecond(second, out first))
                {
                    return true;
                }
            }

            // Match on case insensitive
            return _caseInsensitiveDictionary.TryGetBySecond(second, out first);
        }

        public bool TryGetByFirstCaseSensitive(string first, out string second)
        {
            if (TryGetByFirst(first, out second) && TryGetBySecond(second, out string compareWithFirst))
            {
                if (first.Equals(compareWithFirst, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            second = null;
            return false;
        }

        public bool TryGetBySecondCaseSensitive(string second, out string first)
        {
            if (TryGetBySecond(second, out first) && TryGetByFirst(first, out string compareWithSecond))
            {
                if (second.Equals(compareWithSecond, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            second = null;
            return false;
        }
    }
}