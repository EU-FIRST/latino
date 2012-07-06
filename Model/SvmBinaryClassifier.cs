/*==========================================================================;
 *
 *  This file is part of LATINO. See http://www.latinolib.org
 *
 *  File:    SvmBinaryClassifier.cs
 *  Desc:    SVM Light LATINO wrapper (based on SvmLightLib)
 *  Created: Feb-2011
 *
 *  Author:  Miha Grcar
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Class SvmBinaryClassifier<LblT>
       |
       '-----------------------------------------------------------------------
    */
    public class SvmBinaryClassifier<LblT> : IModel<LblT, SparseVector<double>>, IDisposable
    {
        private int mModelId
            = -1;

        private ArrayList<LblT> mIdxToLbl 
            = new ArrayList<LblT>();
        private IEqualityComparer<LblT> mLblCmp
            = null;

        private SvmLightVerbosityLevel mVerbosityLevel
            = SvmLightVerbosityLevel.Info; // -v 1

        private double mC
            = 0; // default value ([avg. x*x]^-1)
        private bool mBiasedHyperplane
            = true; // -b 1

        private SvmLightKernelType mKernelType
            = SvmLightKernelType.Linear; // -t 0
        private double mKernelParamGamma
            = 1; // -g 1
        private double mKernelParamD
            = 1; // -d 1
        private double mKernelParamS
            = 1; // -s 1
        private double mKernelParamC
            = 0; // -r 0

        private string mCustomParams
            = null;

        public SvmBinaryClassifier(IEqualityComparer<LblT> lblCmp)
        {
            mLblCmp = lblCmp;
        }

        public SvmBinaryClassifier() : this((IEqualityComparer<LblT>)null)
        { 
        }

        public SvmBinaryClassifier(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public SvmLightVerbosityLevel VerbosityLevel
        {
            get { return mVerbosityLevel; }
            set { mVerbosityLevel = value; }
        }

        public double C
        {
            get { return mC; }
            set 
            {
                Utils.ThrowException(value < 0 ? new ArgumentOutOfRangeException("C") : null);
                mC = value; 
            }
        }

        public bool BiasedHyperplane
        {
            get { return mBiasedHyperplane; }
            set { mBiasedHyperplane = value; }
        }

        public SvmLightKernelType KernelType
        {
            get { return mKernelType; }
            set { mKernelType = value; }
        }

        public double KernelParamGamma
        {
            get { return mKernelParamGamma; }
            set 
            {
                Utils.ThrowException(value <= 0 ? new ArgumentOutOfRangeException("KernelParamGamma") : null);
                mKernelParamGamma = value; 
            }
        }

        public double KernelParamD
        {
            get { return mKernelParamD; }
            set { mKernelParamD = value; } // range? 
        }
        
        public double KernelParamS
        {
            get { return mKernelParamD; }
            set { mKernelParamD = value; } // range? 
        }

        public double KernelParamC
        {
            get { return mKernelParamD; }
            set { mKernelParamD = value; } // range? 
        }

        public string CustomParams
        {
            get { return mCustomParams; }
            set { mCustomParams = value; } // null is OK here
        }

        public double GetHyperplaneBias()
        {
            Utils.ThrowException(mModelId == -1 ? new InvalidOperationException() : null);
            return SvmLightLib.GetHyperplaneBias(mModelId);
        }

        //public void Test(LabeledDataset<LblT, SparseVector<double>> dataset)
        //{
        //    for (int i = 0; i < SvmLightLib.GetSupportVectorCount(mModelId); i++)
        //    {
        //        Console.WriteLine("alpha={0}", SvmLightLib.GetSupportVectorAlpha(mModelId, i));
        //        Console.WriteLine(dataset[SvmLightLib.GetSupportVectorIndex(mModelId, i)]);
        //        for (int j = 0; j < SvmLightLib.GetSupportVectorFeatureCount(mModelId, i); j++)
        //        {
        //            Console.Write("{0}:{1} ", SvmLightLib.GetSupportVectorFeature(mModelId, i, j), SvmLightLib.GetSupportVectorWeight(mModelId, i, j));
        //        }
        //        Console.WriteLine();
        //        Console.WriteLine();
        //    }
        //}

        public ArrayList<IdxDat<double>> GetAlphas() // returns pairs (support vector index, alpha * y)
        {
            Utils.ThrowException(mModelId == -1 ? new InvalidOperationException() : null);
            ArrayList<IdxDat<double>> alphas = new ArrayList<IdxDat<double>>();
            for (int i = 0; i < SvmLightLib.GetSupportVectorCount(mModelId); i++)
            {
                double alpha = SvmLightLib.GetSupportVectorAlpha(mModelId, i);
                int idx = SvmLightLib.GetSupportVectorIndex(mModelId, i);
                alphas.Add(new IdxDat<double>(idx, alpha));
            }
            return alphas;
        }        

        public double[] GetLinearWeights()
        {
            Utils.ThrowException(mModelId == -1 ? new InvalidOperationException() : null);
            Utils.ThrowException(mKernelType != SvmLightKernelType.Linear ? new InvalidOperationException() : null);
            int featureCount = SvmLightLib.GetFeatureCount(mModelId);
            double[] weights = new double[featureCount];
            for (int i = 0; i < featureCount; i++)
            {
                weights[i] = SvmLightLib.GetLinearWeight(mModelId, i);
            }
            return weights;
        }

        //public double ClassifyLinear(SparseVector<double> x) // D(x) = w dot x - b
        //{
        //    double b = GetHyperplaneBias();
        //    double[] w = GetLinearWeights(); 
        //    double result = 0;
        //    foreach (IdxDat<double> xi in x) { result += xi.Dat * w[xi.Idx]; }
        //    return result - b;
        //}

        private SparseVector<double> GetSupportVector(int idx)
        {
            int featCount = SvmLightLib.GetSupportVectorFeatureCount(mModelId, idx);
            SparseVector<double> vec = new SparseVector<double>();
            for (int i = 0; i < featCount; i++)
            {
                vec.InnerIdx.Add(SvmLightLib.GetSupportVectorFeature(mModelId, idx, i));
                vec.InnerDat.Add(SvmLightLib.GetSupportVectorWeight(mModelId, idx, i));
            }
            return vec;
        }

        //public double Classify(SparseVector<double> x) // D(x) = SUM_i(kernel(sv_i, x) * (alpha_i * y_i)) - b
        //{
        //    double b = GetHyperplaneBias();
        //    ArrayList<IdxDat<double>> alphas = GetAlphas();
        //    double result = 0;
        //    int i = 0;
        //    foreach (IdxDat<double> svInfo in alphas)
        //    {
        //        result += DotProductSimilarity.Instance.GetSimilarity(GetSupportVector(i++), x) * svInfo.Dat; // *** works only for linear kernels
        //    }
        //    return result - b;
        //}

        private double Mult(double[][] m1, double[][] m2, int row1, int col2)
        {
            double[] row = m1[row1];
            double val = 0;
            for (int i = 0; i < row.Length; i++)
            {
                val += row[i] * m2[i][col2];
            }
            return val;
        }

        private double[][] MatrixMultiply(double[][] m1, double[][] m2)
        {
            double[][] m = new double[m1.Length][];
            for (int row1 = 0; row1 < m1.Length; row1++)
            {
                double[] row = new double[m2[0].Length];
                for (int col2 = 0; col2 < m2[0].Length; col2++)
                {
                    row[col2] = Mult(m1, m2, row1, col2);
                }
                m[row1] = row;
            }
            return m;
        }

        //public void TestMatrixMult()
        //{
        //    double[][] a = new double[][] { 
        //        new double[] { 1, 3, 2, 4 }, 
        //        new double[] { -1, -4, -3, -2 } };
        //    double[][] b = new double[][] { 
        //        new double[] { 0, 1, 6, 5 }, 
        //        new double[] { 2, 3, 4, 7 }, 
        //        new double[] { 8, -1, 9, -3 }, 
        //        new double[] { -2, 10, 11, 12 } };
        //    double[][] m = MatrixMultiply(a, b);
        //    // ( 14 48 80 68 )
        //    // ( -28 -30 -71 -48 )
        //    foreach (double[] row in m)
        //    {
        //        Console.WriteLine(new ArrayList<double>(row));
        //    }
        //}

        private double ComputeAlphaTHAlpha(int rmvFeatIdx) // computes alphaT * H * alpha, where H = [yi * yj * K(xi, xj)]
        {             
            ArrayList<IdxDat<double>> alphas = GetAlphas();
            double[][] alphaT = new double[1][];
            double[][] alpha = new double[alphas.Count][];
            alphaT[0] = new double[alphas.Count];
            for (int i = 0; i < alphas.Count; i++) 
            {
                alpha[i] = new double[] { alphas[i].Dat };
                alphaT[0][i] = alphas[i].Dat; 
            }
            double[][] k = GetKernel(rmvFeatIdx);
            double[][] m = MatrixMultiply(MatrixMultiply(alphaT, k), alpha);
            return m[0][0];
        }

        public ArrayList<KeyDat<double, int>> RankFeatures() // Guyon et al. 2002
        {
            Utils.ThrowException(mModelId == -1 ? new InvalidOperationException() : null);
            ArrayList<KeyDat<double, int>> result = new ArrayList<KeyDat<double, int>>();
            if (mKernelType != SvmLightKernelType.Linear)
            {
                // any kernel
                int numFeat = SvmLightLib.GetFeatureCount(mModelId);
                double allFeat = 0.5 * ComputeAlphaTHAlpha(-1);
                for (int i = 0; i < numFeat; i++)
                {
                    double featScore = Math.Abs(allFeat - 0.5 * ComputeAlphaTHAlpha(/*rmvFeatIdx=*/i));
                    result.Add(new KeyDat<double, int>(featScore, i));
                    //Console.WriteLine(result.Last);
                }
            }
            else
            {
                // linear kernel (fast)
                double[] w = GetLinearWeights();
                for (int i = 0; i < w.Length; i++)
                {
                    result.Add(new KeyDat<double, int>(0.5 * w[i] * w[i], i));
                    //Console.WriteLine(result.Last);
                }
            }
            result.Sort(DescSort<KeyDat<double, int>>.Instance);
            return result;
        }

        private double[][] GetKernel(int rmvFeatIdx)
        {
            int numSv = SvmLightLib.GetSupportVectorCount(mModelId);
            // initialize matrix
            double[][] kernel = new double[numSv][];
            // compute linear kernel
            SparseMatrix<double> m = new SparseMatrix<double>();
            for (int i = 0; i < numSv; i++)
            {
                SparseVector<double> sv = GetSupportVector(i);
                m[i] = sv;
            }
            if (rmvFeatIdx >= 0) { m.RemoveColAt(rmvFeatIdx); } 
            SparseMatrix<double> mTr = m.GetTransposedCopy();
            for (int i = 0; i < numSv; i++)
            {
                double[] innerProd = ModelUtils.GetDotProductSimilarity(mTr, numSv, m[i]);
                kernel[i] = innerProd;
            }
            // compute non-linear kernel
            switch (mKernelType)
            { 
                case SvmLightKernelType.Polynomial:
                    for (int row = 0; row < kernel.Length; row++)
                    {
                        for (int col = 0; col < kernel.Length; col++)
                        {
                            kernel[row][col] = Math.Pow(mKernelParamS * kernel[row][col] + mKernelParamC, mKernelParamD);
                        }
                    }
                    break;
                case SvmLightKernelType.RadialBasisFunction:
                    double[] diag = new double[kernel.Length];
                    for (int i = 0; i < kernel.Length; i++) { diag[i] = kernel[i][i]; } // save diagonal
                    for (int row = 0; row < kernel.Length; row++)
                    {
                        for (int col = 0; col < kernel.Length; col++)
                        {
                            kernel[row][col] = Math.Exp(-mKernelParamGamma * (diag[row] + diag[col] - 2.0 * kernel[row][col]));
                        }
                    }
                    break;
                case SvmLightKernelType.Sigmoid:
                    for (int row = 0; row < kernel.Length; row++)
                    {
                        for (int col = 0; col < kernel.Length; col++)
                        {
                            kernel[row][col] = Math.Tanh(mKernelParamS * kernel[row][col] + mKernelParamC);
                        }
                    }
                    break;
            }
            return kernel;
        }

        // *** IModel<LblT, SparseVector<double>> interface implementation ***

        public Type RequiredExampleType
        {
            get { return typeof(SparseVector<double>); }
        }

        public bool IsTrained
        {
            get { return mModelId != -1; }
        }

        public void Train(ILabeledExampleCollection<LblT, SparseVector<double>> dataset)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            Utils.ThrowException(dataset.Count == 0 ? new ArgumentValueException("dataset") : null);
            Dispose();
            int[] trainSet = new int[dataset.Count];
            int[] labels = new int[dataset.Count];
            Dictionary<LblT, int> lblToIdx = new Dictionary<LblT, int>();
            int j = 0;
            foreach (LabeledExample<LblT, SparseVector<double>> lblEx in dataset)
            { 
                SparseVector<double> vec = lblEx.Example;
                int[] idx = new int[vec.Count];
                float[] val = new float[vec.Count];
                for (int i = 0; i < vec.Count; i++)
                {
                    idx[i] = vec.InnerIdx[i] + 1; 
                    val[i] = (float)vec.InnerDat[i]; // *** cast to float
                }
                int lbl;
                if (!lblToIdx.TryGetValue(lblEx.Label, out lbl))
                {
                    lblToIdx.Add(lblEx.Label, lbl = lblToIdx.Count);
                    mIdxToLbl.Add(lblEx.Label);
                }
                Utils.ThrowException(lbl == 2 ? new ArgumentValueException("dataset") : null);
                trainSet[j++] = SvmLightLib.NewFeatureVector(idx.Length, idx, val, lbl == 0 ? 1 : -1);
            }
            mModelId = SvmLightLib.TrainModel(string.Format("-v {0} -c {1} -t {2} -g {3} -d {4} -s {5} -r {6} -b {7} {8}", 
                (int)mVerbosityLevel, mC, (int)mKernelType, mKernelParamGamma, mKernelParamD, mKernelParamS, mKernelParamC, mBiasedHyperplane ? 1 : 0,
                mCustomParams), trainSet.Length, trainSet);
            // delete training vectors 
            foreach (int vecIdx in trainSet) { SvmLightLib.DeleteFeatureVector(vecIdx); }
        }

        void IModel<LblT>.Train(ILabeledExampleCollection<LblT> dataset)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            Utils.ThrowException(!(dataset is ILabeledExampleCollection<LblT, SparseVector<double>>) ? new ArgumentTypeException("dataset") : null);
            Train((ILabeledExampleCollection<LblT, SparseVector<double>>)dataset); // throws ArgumentValueException
        }

        public Prediction<LblT> Predict(SparseVector<double> example)
        {
            Utils.ThrowException(mModelId == -1 ? new InvalidOperationException() : null);
            Utils.ThrowException(example == null ? new ArgumentNullException("example") : null);
            Prediction<LblT> result = new Prediction<LblT>();
            int[] idx = new int[example.Count];
            float[] val = new float[example.Count];
            for (int i = 0; i < example.Count; i++)
            {
                idx[i] = example.InnerIdx[i] + 1; 
                val[i] = (float)example.InnerDat[i]; // *** cast to float
            }
            int vecId = SvmLightLib.NewFeatureVector(idx.Length, idx, val, 0);
            SvmLightLib.Classify(mModelId, 1, new int[] { vecId });
            double score = SvmLightLib.GetFeatureVectorClassifScore(vecId, 0);
            LblT lbl = mIdxToLbl[score > 0 ? 0 : 1];
            LblT otherLbl = mIdxToLbl[score > 0 ? 1 : 0];
            result.Inner.Add(new KeyDat<double, LblT>(Math.Abs(score), lbl));
            result.Inner.Add(new KeyDat<double, LblT>(-Math.Abs(score), otherLbl));
            SvmLightLib.DeleteFeatureVector(vecId); // delete feature vector
            return result;
        }

        Prediction<LblT> IModel<LblT>.Predict(object example)
        {
            Utils.ThrowException(example == null ? new ArgumentNullException("example") : null);
            Utils.ThrowException(!(example is SparseVector<double>) ? new ArgumentTypeException("example") : null);
            return Predict((SparseVector<double>)example); // throws InvalidOperationException
        }

        // *** IDisposable interface implementation ***

        public void Dispose()
        {
            if (mModelId != -1)
            {
                SvmLightLib.DeleteModel(mModelId);
                mIdxToLbl.Clear();
                mModelId = -1;
            }
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            writer.WriteInt((int)mVerbosityLevel);
            writer.WriteDouble(mC);
            writer.WriteBool(mBiasedHyperplane);
            writer.WriteInt((int)mKernelType);
            writer.WriteDouble(mKernelParamGamma);
            writer.WriteDouble(mKernelParamD);
            writer.WriteDouble(mKernelParamS);
            writer.WriteDouble(mKernelParamC);
            writer.WriteString(mCustomParams);
            //writer.WriteDouble(mEps);
            mIdxToLbl.Save(writer);
            writer.WriteObject(mLblCmp);
            writer.WriteBool(mModelId != -1);
            if (mModelId != -1)
            {
                SvmLightLib.WriteByteCallback wb = delegate(byte b) { writer.WriteByte(b); };
                SvmLightLib.SaveModelBinCallback(mModelId, wb);
                GC.KeepAlive(wb);
            }
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            Dispose();
            // the following statements throw serialization-related exceptions
            mVerbosityLevel = (SvmLightVerbosityLevel)reader.ReadInt();
            mC = reader.ReadDouble();
            mBiasedHyperplane = reader.ReadBool();
            mKernelType = (SvmLightKernelType)reader.ReadInt();
            mKernelParamGamma = reader.ReadDouble();
            mKernelParamD = reader.ReadDouble();
            mKernelParamS = reader.ReadDouble();
            mKernelParamC = reader.ReadDouble();
            mCustomParams = reader.ReadString();
            //mEps = reader.ReadDouble();
            mIdxToLbl.Load(reader);
            mLblCmp = reader.ReadObject<IEqualityComparer<LblT>>();
            if (reader.ReadBool())
            {
                SvmLightLib.ReadByteCallback rb = delegate() { return reader.ReadByte(); };
                mModelId = SvmLightLib.LoadModelBinCallback(rb);
                GC.KeepAlive(rb);
            }
        }
    }
}