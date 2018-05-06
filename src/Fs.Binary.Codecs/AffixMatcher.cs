using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Fs.Binary.Codecs
{
    internal static class AffixMatcher
    {
        public static int TryMatchAffix ( ReadOnlySpan<char> inputData, int inputIndex, int inputEnd, bool flush, ImmutableArray<string> affixes )
        {
            // affixes must match exactly, we do not skip characters in the input. The incoming list of affixes is
            // sorted from shortest to longest and there are no duplicates. The sort order lets us mark matches as
            // we find them and know that we always select the longest (best) match.

            int initialInputIndex = inputIndex;
            int affixesLength = affixes.Length;
            int matchOffset = 0;
            int matchLength = 0;
            int partialMatchIndices = (1 << affixesLength) - 1;

            while (inputIndex < inputEnd)
            {
                char inputChar = inputData[inputIndex];
                if (inputChar >= 128)
                {
                    // nothing else can match at this point...
                    partialMatchIndices = 0;
                    break;
                }

                for (int affixIndex = 0; affixIndex < affixesLength; affixIndex++)
                {
                    // ignore affixes that we've already determined don't match..
                    if ((partialMatchIndices & (1 << affixIndex)) == 0)
                        continue;

                    var affixString = affixes[affixIndex];
                    if (affixString[matchOffset] == inputChar)
                    {
                        if (affixString.Length != matchOffset + 1)
                            continue;

                        matchLength = affixString.Length;
                    }

                    // complete match or non-match, either way remove from partials..
                    if ((partialMatchIndices &= ~(1 << affixIndex)) == 0)
                        break;
                }

                // next input character..
                matchOffset++;
                inputIndex++;
            }

            // if we're flushing, remaining possible matches aren't really possible..
            if (flush) partialMatchIndices = 0;

            // if no more matching, whatever matched wins,.. otherwise not conclusive..
            return (partialMatchIndices == 0) ? matchLength : -1;
        }
    }
}
