/*==========================================================================;
 *
 *  This file is part of LATINO. See http://www.latinolib.org
 *
 *  File:    CosineSimilarity.cs
 *  Desc:    Cosine similarity measure
 *  Created: Dec-2008
 *
 *  Authors: Miha Grcar, Matjaz Jursic
 *
 *  License: GNU LGPL (http://www.gnu.org/licenses/lgpl.txt)
 *
 ***************************************************************************/

using System;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Class CosineSimilarity
       |
       '-----------------------------------------------------------------------
    */
    public class CosineSimilarity : ISimilarity<SparseVector<double>.ReadOnly>, ISimilarity<SparseVector<double>>
    {
        public static CosineSimilarity mInstance
            = new CosineSimilarity();

        public CosineSimilarity()
        {
        }

        public CosineSimilarity(BinarySerializer reader)
        {
        }

        public static CosineSimilarity Instance
        {
            get { return mInstance; }
        }

        // *** ISimilarity<SparseVector<double>> interface implementation ***

        public double GetSimilarity(SparseVector<double> a, SparseVector<double> b)
        {
            return GetSimilarity(new SparseVector<double>.ReadOnly(a), new SparseVector<double>.ReadOnly(b));
        }

        // *** ISimilarity<SparseVector<double>.ReadOnly> interface implementation ***

        public double GetSimilarity(SparseVector<double>.ReadOnly a, SparseVector<double>.ReadOnly b)
        {
            Utils.ThrowException(a == null ? new ArgumentNullException("a") : null);
            Utils.ThrowException(b == null ? new ArgumentNullException("b") : null);
            double dotProd = 0;
            int i = 0, j = 0;
            int aCount = a.Count;
            int bCount = b.Count;
            if (aCount == 0 || bCount == 0) { return 0; }
            ArrayList<int> aIdx = a.Inner.InnerIdx;
            ArrayList<double> aDat = a.Inner.InnerDat;
            ArrayList<int> bIdx = b.Inner.InnerIdx;
            ArrayList<double> bDat = b.Inner.InnerDat;
            int aIdx_i = aIdx[0];
            int bIdx_j = bIdx[0];
            while (true)
            {
                if (aIdx_i < bIdx_j)
                {
                    if (++i == aCount) { break; }
                    aIdx_i = aIdx[i];
                }
                else if (aIdx_i > bIdx_j)
                {
                    if (++j == bCount) { break; }
                    bIdx_j = bIdx[j];
                }
                else
                {
                    dotProd += aDat[i] * bDat[j];
                    if (++i == aCount || ++j == bCount) { break; }
                    aIdx_i = aIdx[i];
                    bIdx_j = bIdx[j];
                }
            }
            double aLen = ModelUtils.GetVecLenL2(a);
            double bLen = ModelUtils.GetVecLenL2(b);
            double lenMult = aLen * bLen;
            if (lenMult == 0)
            {
                // lenMult == 0 implies that either a, b, or both are zero length. Zero length can only happen when all
                // components of a vector are 0. Such zeroed vector can not be similar to any other vector, therefore sim = 0
                return 0;
            } 
            return dotProd / lenMult;
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
        }
    }
}