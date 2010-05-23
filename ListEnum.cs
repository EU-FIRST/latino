﻿/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:    ListEnum.cs
 *  Desc:    Generic list enumerator
 *  Created: Nov-2007
 *
 *  Authors: Miha Grcar
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Class ListEnum
       |
       '-----------------------------------------------------------------------
    */
    public class ListEnum : IEnumerator
    {
        private IEnumerableList mList;
        private int mCurrentIdx
            = -1;

        public ListEnum(IEnumerableList list)
        {
            Utils.ThrowException(list == null ? new ArgumentNullException("list") : null);
            mList = list;
        }

        // *** IEnumerator interface implementation ***

        public void Reset()
        {
            mCurrentIdx = -1;
        }

        public object Current
        {
            get { return mList[mCurrentIdx]; } // throws ArgumentOutOfRangeException
        }

        public bool MoveNext()
        {
            mCurrentIdx++;
            if (mCurrentIdx >= mList.Count) 
            { 
                Reset(); 
                return false; 
            }
            return true;
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class ListEnum<T>
       |
       '-----------------------------------------------------------------------
    */
    public class ListEnum<T> : IEnumerator<T>
    {
        private IEnumerableList<T> mList;
        private int mCurrentIdx
            = -1;

        public ListEnum(IEnumerableList<T> list)
        {
            Utils.ThrowException(list == null ? new ArgumentNullException("list") : null);
            mList = list;
        }

        // *** IEnumerator<T> interface implementation ***

        public void Reset()
        {
            mCurrentIdx = -1;
        }

        public T Current
        {
            get { return mList[mCurrentIdx]; } // throws ArgumentOutOfRangeException
        }

        object IEnumerator.Current
        {
            get { return Current; } // throws ArgumentOutOfRangeException
        }

        public bool MoveNext()
        {
            mCurrentIdx++;
            if (mCurrentIdx >= mList.Count) 
            { 
                Reset(); 
                return false; 
            }
            return true;
        }

        public void Dispose()
        {
        }
    }
}