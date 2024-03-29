﻿/*==========================================================================;
 *
 *  This file is part of LATINO. See http://www.latinolib.org
 *
 *  File:    SemanticSpaceLayout.cs
 *  Desc:    Semantic space layout algorithm
 *  Created: Nov-2009
 *
 *  Author:  Miha Grcar
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using Latino.Model;

namespace Latino.Visualization
{
    /* .-----------------------------------------------------------------------
       |
       |  Class SemanticSpaceLayout 
       |
       '-----------------------------------------------------------------------
    */
    public class SemanticSpaceLayout : ILayoutAlgorithm
    {
        private IUnlabeledExampleCollection<SparseVector<double>> mDataset;
        private Random mRandom
            = new Random(1);
        private double mKMeansEps
            = 0.01;
        private int mKClust
            = 100;
        private double mSimThresh
            = 0.005;
        private int mKNN
            = 10;

        private static Logger mLogger
            = Logger.GetLogger(typeof(SemanticSpaceLayout));

        public SemanticSpaceLayout(IUnlabeledExampleCollection<SparseVector<double>> dataset)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            mDataset = dataset;
        }

        public Random Random
        {
            get { return mRandom; }
            set 
            {
                Utils.ThrowException(value == null ? new ArgumentNullException("Random") : null);
                mRandom = value; 
            }
        }

        public double KMeansEps
        {
            get { return mKMeansEps; }
            set
            {
                Utils.ThrowException(value < 0 ? new ArgumentOutOfRangeException("KMeansEps") : null);
                mKMeansEps = value;
            }
        }

        public int KMeansK
        {
            get { return mKClust; }
            set
            {
                Utils.ThrowException(value < 2 ? new ArgumentOutOfRangeException("KMeansK") : null);
                mKClust = value;
            }
        }

        public double SimThresh
        {
            get { return mSimThresh; }
            set 
            {
                Utils.ThrowException(value < 0 ? new ArgumentOutOfRangeException("SimThresh") : null);
                mSimThresh = value;
            }
        }

        public int NeighborhoodSize
        {
            get { return mKNN; }
            set
            {
                Utils.ThrowException(value < 1 ? new ArgumentOutOfRangeException("NeighborhoodSize") : null);
                mKNN = value;
            }
        }

        // *** ILayoutAlgorithm interface implementation ***

        public Vector2D[] ComputeLayout()
        {
            return ComputeLayout(/*settings=*/null);
        }

        public Vector2D[] ComputeLayout(LayoutSettings settings)
        {
            UnlabeledDataset<SparseVector<double>> dataset = new UnlabeledDataset<SparseVector<double>>(mDataset);
            // clustering 
            mLogger.Info("ComputeLayout", "Clustering ...");
            KMeansClusteringFast kMeans = new KMeansClusteringFast(mKClust);
            kMeans.Eps = mKMeansEps;
            kMeans.Random = mRandom;
            kMeans.Trials = 1;
            ClusteringResult clustering = kMeans.Cluster(mDataset); // throws ArgumentValueException
            // determine reference instances
            UnlabeledDataset<SparseVector<double>> dsRefInst = new UnlabeledDataset<SparseVector<double>>();
            foreach (Cluster cluster in clustering.Roots)
            {
                SparseVector<double> centroid
                    = cluster.Items.Count > 0 ? cluster.ComputeCentroid(mDataset, CentroidType.NrmL2) : new SparseVector<double>();
                dsRefInst.Add(centroid); // dataset of reference instances
                dataset.Add(centroid); // add centroids to the main dataset
            }
            // position reference instances
            mLogger.Info("ComputeLayout", "Positioning reference instances ...");
            SparseMatrix<double> simMtx = ModelUtils.GetDotProductSimilarity(dsRefInst, mSimThresh, /*fullMatrix=*/false);
            StressMajorizationLayout sm = new StressMajorizationLayout(dsRefInst.Count, new DistFunc(simMtx));
            sm.Random = mRandom;
            Vector2D[] centrPos = sm.ComputeLayout();
            // k-NN
            mLogger.Info("ComputeLayout", "Computing similarities ...");
            simMtx = ModelUtils.GetDotProductSimilarity(dataset, mSimThresh, /*fullMatrix=*/true);
            mLogger.Info("ComputeLayout", "Constructing system of linear equations ...");
            LabeledDataset<double, SparseVector<double>> lsqrDs = new LabeledDataset<double, SparseVector<double>>();
            foreach (IdxDat<SparseVector<double>> simMtxRow in simMtx)
            {
                if (simMtxRow.Dat.Count <= 1)
                {
                    mLogger.Warn("ComputeLayout", "Instance #{0} has no neighborhood.", simMtxRow.Idx);
                }
                ArrayList<KeyDat<double, int>> knn = new ArrayList<KeyDat<double, int>>(simMtxRow.Dat.Count);
                foreach (IdxDat<double> item in simMtxRow.Dat)
                {
                    if (item.Idx != simMtxRow.Idx)
                    {
                        knn.Add(new KeyDat<double, int>(item.Dat, item.Idx));
                    }
                }
                knn.Sort(DescSort<KeyDat<double, int>>.Instance);
                int count = Math.Min(knn.Count, mKNN);
                SparseVector<double> eq = new SparseVector<double>();
                double wgt = 1.0 / (double)count;
                for (int i = 0; i < count; i++)
                {
                    eq.InnerIdx.Add(knn[i].Dat);
                    eq.InnerDat.Add(-wgt);
                }
                eq.InnerIdx.Sort(); // *** sort only indices
                eq[simMtxRow.Idx] = 1;
                lsqrDs.Add(0, eq);
            }
            Vector2D[] layout = new Vector2D[dataset.Count - mKClust];
            for (int i = dataset.Count - mKClust, j = 0; i < dataset.Count; i++, j++)
            {
                SparseVector<double> eq = new SparseVector<double>(new IdxDat<double>[] { new IdxDat<double>(i, 1) });
                lsqrDs.Add(centrPos[j].X, eq);
            }
            LSqrModel lsqr = new LSqrModel();
            lsqr.Train(lsqrDs);
            for (int i = 0; i < layout.Length; i++)
            {
                layout[i].X = lsqr.Solution[i];
            }
            for (int i = lsqrDs.Count - mKClust, j = 0; i < lsqrDs.Count; i++, j++)
            {
                lsqrDs[i].Label = centrPos[j].Y;
            }
            lsqr.Train(lsqrDs);
            for (int i = 0; i < layout.Length; i++)
            {
                layout[i].Y = lsqr.Solution[i];
            }
            return settings == null ? layout : settings.AdjustLayout(layout);
        }

        /* .-----------------------------------------------------------------------
           |
           |  Class DistFunc
           |
           '-----------------------------------------------------------------------
        */
        private class DistFunc : IDistance<int>
        {
            private SparseMatrix<double>.ReadOnly mSimMtx;

            public DistFunc(SparseMatrix<double>.ReadOnly simMtx)
            {
                mSimMtx = simMtx;
            }

            public double GetDistance(int a, int b) 
            {
                if (a > b) { return 1.0 - mSimMtx.TryGet(b, a, 0); }
                else { return 1.0 - mSimMtx.TryGet(a, b, 0); }
            }

            public void Save(BinarySerializer dummy)
            {
                throw new NotImplementedException();
            }
        }
    }
}