﻿/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          Prediction.cs
 *  Version:       1.0
 *  Desc:		   Prediction (output of ML models)
 *  Author:        Miha Grcar
 *  Created on:    Aug-2007
 *  Last modified: Nov-2009
 *  Revision:      Nov-2009
 *
 ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Class Prediction<LblT>
       |
       '-----------------------------------------------------------------------
    */
    public class Prediction<LblT> : IEnumerableList<KeyDat<double, LblT>>
    {
        private ArrayList<KeyDat<double, LblT>> mClassScores
            = new ArrayList<KeyDat<double, LblT>>();
        private static DescSort<KeyDat<double, LblT>> mDescSort
            = new DescSort<KeyDat<double, LblT>>();

        public Prediction()
        {
        }

        public Prediction(IEnumerable<KeyDat<double, LblT>> classScores)
        {
            Utils.ThrowException(classScores == null ? new ArgumentNullException("classScores") : null);
            AddRange(classScores);
        }

        public void AddRange(IEnumerable<KeyDat<double, LblT>> classScores)
        {
            foreach (KeyDat<double, LblT> classScore in classScores)
            {
                mClassScores.Add(classScore);
            }
            mClassScores.Sort(mDescSort);
        }

        public ArrayList<KeyDat<double, LblT>> Items
        {
            get { return mClassScores; }
        }

        public double GetScoreAt(int idx)
        {
            Utils.ThrowException((idx < 0 || idx >= mClassScores.Count) ? new ArgumentOutOfRangeException("idx") : null);
            return mClassScores[idx].Key;
        }

        public LblT GetClassLabelAt(int idx)
        {
            Utils.ThrowException((idx < 0 || idx >= mClassScores.Count) ? new ArgumentOutOfRangeException("idx") : null);
            return mClassScores[idx].Dat;
        }

        public double BestScore
        {
            get
            {
                Utils.ThrowException(mClassScores.Count == 0 ? new InvalidOperationException() : null);
                return mClassScores[0].Key;
            }
        }

        public LblT BestClassLabel
        {
            get
            {
                Utils.ThrowException(mClassScores.Count == 0 ? new InvalidOperationException() : null);
                return mClassScores[0].Dat;
            }
        }

        public override string ToString()
        {
            return mClassScores.ToString();
        }

        // *** IEnumerableList<KeyDat<double, LblT>> interface implementation ***

        public int Count
        {
            get { return mClassScores.Count; }
        }

        public KeyDat<double, LblT> this[int idx]
        {
            get
            {
                Utils.ThrowException((idx < 0 || idx >= mClassScores.Count) ? new ArgumentOutOfRangeException("idx") : null);
                return mClassScores[idx];
            }
        }

        object IEnumerableList.this[int idx]
        {
            get { return this[idx]; } // throws ArgumentOutOfRangeException
        }

        public IEnumerator<KeyDat<double, LblT>> GetEnumerator()
        {
            return new ListEnum<KeyDat<double, LblT>>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ListEnum(this);
        }
    }
}