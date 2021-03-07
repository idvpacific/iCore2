using Accord.Video;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace iCore_Administrator.API.Modules
{
    public class SimilarityFunction
    {
        //----------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------
        private int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;
            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;
            if (sourceWordCount == 0) { return targetWordCount; }
            if (targetWordCount == 0) { return sourceWordCount; }
            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }
            return distance[sourceWordCount, targetWordCount];
        }
        private double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;
            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
        public int GetWordsSimilarity(string Word1, string Word2)
        {
            try
            {
                return (int)(Math.Round(CalculateSimilarity(Word1, Word2) * 100));
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public int GetWordsSimilarityInString(string Word1, string FullText)
        {
            try
            {
                int MaxSimilarityPercent = 0;
                string[] WordArray = FullText.Split(' ');
                for (int i = 0; i < WordArray.Length; i++)
                {
                    int LocalSimilarityPercent = (int)(Math.Round(CalculateSimilarity(Word1.ToUpper(), WordArray[i].ToUpper()) * 100));
                    if (LocalSimilarityPercent > MaxSimilarityPercent) { MaxSimilarityPercent = LocalSimilarityPercent; }
                }
                return MaxSimilarityPercent;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        //----------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------
        public int GetColorSimilarity(Color Color1, Color Color2)
        {
            try
            {
                int diffRed = Math.Abs(Color1.R - Color2.R);
                int diffGreen = Math.Abs(Color1.G - Color2.G);
                int diffBlue = Math.Abs(Color1.B - Color2.B);
                float pctDiffRed = (float)diffRed / 255;
                float pctDiffGreen = (float)diffGreen / 255;
                float pctDiffBlue = (float)diffBlue / 255;
                return (int)(100 - Math.Round((pctDiffRed + pctDiffGreen + pctDiffBlue) / 3 * 100));
            }
            catch (Exception)
            {
                return 0;
            }
        }
        //----------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------
        public bool KeyDetector(ref int X, ref int Y, string Key, IList<Line> Lines, string Similarity, int KeyIndex, string KeyPosition)
        {
            try
            {
                bool resTF = false;
                KeyPosition = KeyPosition.Trim().ToUpper();
                X = -1; Y = -1;
                Key = Key.ToLower().Trim();
                if (KeyIndex <= 0) { KeyIndex = 1; }
                Similarity = Similarity.Trim();
                int SimilarityInt = int.Parse(Similarity);
                int KeyFounded = 0;
                Line SelectedLineKey = null;
                Line SelectedLineKeySec = null;
                Line SelectedLineKeyLst = null;
                foreach (Line LN in Lines)
                {
                    foreach (Word WD in LN.Words)
                    {
                        if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                        {
                            KeyFounded++;
                            if (KeyFounded >= KeyIndex)
                            {
                                SelectedLineKeySec = LN;
                                break;
                            }
                            else
                            {
                                SelectedLineKey = LN;
                            }
                        }
                        if (SelectedLineKeySec != null) { break; }
                    }
                    if (SelectedLineKeySec != null) { break; }
                }
                if ((SelectedLineKey != null) && (SelectedLineKeySec == null)) { SelectedLineKeyLst = SelectedLineKey; }
                if ((SelectedLineKey == null) && (SelectedLineKeySec != null)) { SelectedLineKeyLst = SelectedLineKeySec; }
                if ((SelectedLineKey != null) && (SelectedLineKeySec != null))
                {
                    switch (KeyPosition)
                    {
                        case "T":
                            {
                                if (SelectedLineKey.MinTop <= SelectedLineKeySec.MinTop)
                                {
                                    SelectedLineKeyLst = SelectedLineKey;
                                }
                                else
                                {
                                    SelectedLineKeyLst = SelectedLineKeySec;
                                }
                                break;
                            }
                        //-----------------------------------------------
                        case "TL":
                            {
                                if (SelectedLineKey.MinTop < SelectedLineKeySec.MinTop)
                                {
                                    SelectedLineKeyLst = SelectedLineKey;
                                }
                                else
                                {
                                    if (SelectedLineKey.MinTop == SelectedLineKeySec.MinTop)
                                    {
                                        int XK = 0;
                                        int XKS = 0;
                                        foreach (Word WD in SelectedLineKey.Words)
                                        {
                                            if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                            {
                                                XK = (int)WD.Left;
                                            }
                                        }
                                        foreach (Word WD in SelectedLineKeySec.Words)
                                        {
                                            if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                            {
                                                XKS = (int)WD.Left;
                                            }
                                        }
                                        if (XK <= XKS)
                                        {
                                            SelectedLineKeyLst = SelectedLineKey;
                                        }
                                        else
                                        {
                                            SelectedLineKeyLst = SelectedLineKeySec;
                                        }
                                    }
                                    else
                                    {
                                        SelectedLineKeyLst = SelectedLineKeySec;
                                    }
                                }
                                break;
                            }
                        //-----------------------------------------------
                        case "TB": { break; } // Wrong Definition 
                        //-----------------------------------------------
                        case "TR":
                            {
                                if (SelectedLineKey.MinTop < SelectedLineKeySec.MinTop)
                                {
                                    SelectedLineKeyLst = SelectedLineKey;
                                }
                                else
                                {
                                    if (SelectedLineKey.MinTop == SelectedLineKeySec.MinTop)
                                    {
                                        int XK = 0;
                                        int XKS = 0;
                                        foreach (Word WD in SelectedLineKey.Words)
                                        {
                                            if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                            {
                                                XK = (int)WD.Left;
                                            }
                                        }
                                        foreach (Word WD in SelectedLineKeySec.Words)
                                        {
                                            if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                            {
                                                XKS = (int)WD.Left;
                                            }
                                        }
                                        if (XK >= XKS)
                                        {
                                            SelectedLineKeyLst = SelectedLineKey;
                                        }
                                        else
                                        {
                                            SelectedLineKeyLst = SelectedLineKeySec;
                                        }
                                    }
                                    else
                                    {
                                        SelectedLineKeyLst = SelectedLineKeySec;
                                    }
                                }
                                break;
                            }
                        //-----------------------------------------------
                        case "L":
                            {
                                int XK = 0;
                                int XKS = 0;
                                foreach (Word WD in SelectedLineKey.Words)
                                {
                                    if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                    {
                                        XK = (int)WD.Left;
                                    }
                                }
                                foreach (Word WD in SelectedLineKeySec.Words)
                                {
                                    if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                    {
                                        XKS = (int)WD.Left;
                                    }
                                }
                                if (XK <= XKS)
                                {
                                    SelectedLineKeyLst = SelectedLineKey;
                                }
                                else
                                {
                                    SelectedLineKeyLst = SelectedLineKeySec;
                                }
                                break;
                            }
                        //-----------------------------------------------
                        case "LT":
                            {
                                int XK = 0;
                                int XKS = 0;
                                foreach (Word WD in SelectedLineKey.Words)
                                {
                                    if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                    {
                                        XK = (int)WD.Left;
                                    }
                                }
                                foreach (Word WD in SelectedLineKeySec.Words)
                                {
                                    if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                    {
                                        XKS = (int)WD.Left;
                                    }
                                }
                                if (XK < XKS)
                                {
                                    SelectedLineKeyLst = SelectedLineKey;
                                }
                                else
                                {
                                    if (XK == XKS)
                                    {
                                        if (SelectedLineKey.MinTop <= SelectedLineKeySec.MinTop)
                                        {
                                            SelectedLineKeyLst = SelectedLineKey;
                                        }
                                        else
                                        {
                                            SelectedLineKeyLst = SelectedLineKeySec;
                                        }
                                    }
                                    else
                                    {
                                        SelectedLineKeyLst = SelectedLineKeySec;
                                    }
                                }
                                break;
                            }
                        //-----------------------------------------------
                        case "LB":
                            {
                                int XK = 0;
                                int XKS = 0;
                                foreach (Word WD in SelectedLineKey.Words)
                                {
                                    if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                    {
                                        XK = (int)WD.Left;
                                    }
                                }
                                foreach (Word WD in SelectedLineKeySec.Words)
                                {
                                    if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                    {
                                        XKS = (int)WD.Left;
                                    }
                                }
                                if (XK < XKS)
                                {
                                    SelectedLineKeyLst = SelectedLineKey;
                                }
                                else
                                {
                                    if (XK == XKS)
                                    {
                                        if (SelectedLineKey.MinTop >= SelectedLineKeySec.MinTop)
                                        {
                                            SelectedLineKeyLst = SelectedLineKey;
                                        }
                                        else
                                        {
                                            SelectedLineKeyLst = SelectedLineKeySec;
                                        }
                                    }
                                    else
                                    {
                                        SelectedLineKeyLst = SelectedLineKeySec;
                                    }
                                }
                                break;
                            }
                        //-----------------------------------------------
                        case "LR": { break; } // Wrong Definition 
                        //-----------------------------------------------
                        case "R":
                            {
                                int XK = 0;
                                int XKS = 0;
                                foreach (Word WD in SelectedLineKey.Words)
                                {
                                    if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                    {
                                        XK = (int)WD.Left;
                                    }
                                }
                                foreach (Word WD in SelectedLineKeySec.Words)
                                {
                                    if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                    {
                                        XKS = (int)WD.Left;
                                    }
                                }
                                if (XK >= XKS)
                                {
                                    SelectedLineKeyLst = SelectedLineKey;
                                }
                                else
                                {
                                    SelectedLineKeyLst = SelectedLineKeySec;
                                }
                                break;
                            }
                        //-----------------------------------------------
                        case "RT":
                            {
                                int XK = 0;
                                int XKS = 0;
                                foreach (Word WD in SelectedLineKey.Words)
                                {
                                    if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                    {
                                        XK = (int)WD.Left;
                                    }
                                }
                                foreach (Word WD in SelectedLineKeySec.Words)
                                {
                                    if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                    {
                                        XKS = (int)WD.Left;
                                    }
                                }
                                if (XK > XKS)
                                {
                                    SelectedLineKeyLst = SelectedLineKey;
                                }
                                else
                                {
                                    if (XK == XKS)
                                    {
                                        if (SelectedLineKey.MinTop <= SelectedLineKeySec.MinTop)
                                        {
                                            SelectedLineKeyLst = SelectedLineKey;
                                        }
                                        else
                                        {
                                            SelectedLineKeyLst = SelectedLineKeySec;
                                        }
                                    }
                                    else
                                    {
                                        SelectedLineKeyLst = SelectedLineKeySec;
                                    }
                                }
                                break;
                            }
                        //-----------------------------------------------
                        case "RL": { break; } // Wrong Definition 
                        //-----------------------------------------------
                        case "RB":
                            {
                                int XK = 0;
                                int XKS = 0;
                                foreach (Word WD in SelectedLineKey.Words)
                                {
                                    if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                    {
                                        XK = (int)WD.Left;
                                    }
                                }
                                foreach (Word WD in SelectedLineKeySec.Words)
                                {
                                    if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                    {
                                        XKS = (int)WD.Left;
                                    }
                                }
                                if (XK > XKS)
                                {
                                    SelectedLineKeyLst = SelectedLineKey;
                                }
                                else
                                {
                                    if (XK == XKS)
                                    {
                                        if (SelectedLineKey.MinTop >= SelectedLineKeySec.MinTop)
                                        {
                                            SelectedLineKeyLst = SelectedLineKey;
                                        }
                                        else
                                        {
                                            SelectedLineKeyLst = SelectedLineKeySec;
                                        }
                                    }
                                    else
                                    {
                                        SelectedLineKeyLst = SelectedLineKeySec;
                                    }
                                }
                                break;
                            }
                        //-----------------------------------------------
                        case "B":
                            {
                                if (SelectedLineKey.MinTop >= SelectedLineKeySec.MinTop)
                                {
                                    SelectedLineKeyLst = SelectedLineKey;
                                }
                                else
                                {
                                    SelectedLineKeyLst = SelectedLineKeySec;
                                }
                                break;
                            }
                        //-----------------------------------------------
                        case "BT": { break; } // Wrong Definition 
                        //-----------------------------------------------
                        case "BL":
                            {
                                if (SelectedLineKey.MinTop > SelectedLineKeySec.MinTop)
                                {
                                    SelectedLineKeyLst = SelectedLineKey;
                                }
                                else
                                {
                                    if (SelectedLineKey.MinTop == SelectedLineKeySec.MinTop)
                                    {
                                        int XK = 0;
                                        int XKS = 0;
                                        foreach (Word WD in SelectedLineKey.Words)
                                        {
                                            if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                            {
                                                XK = (int)WD.Left;
                                            }
                                        }
                                        foreach (Word WD in SelectedLineKeySec.Words)
                                        {
                                            if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                            {
                                                XKS = (int)WD.Left;
                                            }
                                        }
                                        if (XK <= XKS)
                                        {
                                            SelectedLineKeyLst = SelectedLineKey;
                                        }
                                        else
                                        {
                                            SelectedLineKeyLst = SelectedLineKeySec;
                                        }
                                    }
                                    else
                                    {
                                        SelectedLineKeyLst = SelectedLineKeySec;
                                    }
                                }
                                break;
                            }
                        //-----------------------------------------------
                        case "BR":
                            {
                                if (SelectedLineKey.MinTop > SelectedLineKeySec.MinTop)
                                {
                                    SelectedLineKeyLst = SelectedLineKey;
                                }
                                else
                                {
                                    if (SelectedLineKey.MinTop == SelectedLineKeySec.MinTop)
                                    {
                                        int XK = 0;
                                        int XKS = 0;
                                        foreach (Word WD in SelectedLineKey.Words)
                                        {
                                            if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                            {
                                                XK = (int)WD.Left;
                                            }
                                        }
                                        foreach (Word WD in SelectedLineKeySec.Words)
                                        {
                                            if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                                            {
                                                XKS = (int)WD.Left;
                                            }
                                        }
                                        if (XK >= XKS)
                                        {
                                            SelectedLineKeyLst = SelectedLineKey;
                                        }
                                        else
                                        {
                                            SelectedLineKeyLst = SelectedLineKeySec;
                                        }
                                    }
                                    else
                                    {
                                        SelectedLineKeyLst = SelectedLineKeySec;
                                    }
                                }
                                break;
                            }
                        //-----------------------------------------------
                        default:
                            {
                                SelectedLineKeyLst = SelectedLineKey;
                                break;
                            }
                    }
                }
                if (SelectedLineKeyLst != null)
                {
                    double LNTop = SelectedLineKeyLst.MinTop;
                    double LNHeight = SelectedLineKeyLst.MaxHeight;
                    Y = (int)Math.Round(LNTop);
                    foreach (Word WD in SelectedLineKeyLst.Words)
                    {
                        if ((int)(Math.Round(CalculateSimilarity(Key.ToUpper(), WD.WordText.Trim().ToUpper()) * 100)) >= SimilarityInt)
                        {
                            X = (int)WD.Left;
                        }
                    }
                }
                if ((Y >= 0) && (X >= 0)) { resTF = true; }
                return resTF;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}