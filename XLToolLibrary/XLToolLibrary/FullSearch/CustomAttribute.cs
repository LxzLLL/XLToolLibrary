using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XLToolLibrary.Utilities.FullSearch
{
    /// <summary>
    /// 作者：Arvin
    /// 日期：2015/9/11 10:39:05
    /// 描述：自定义的特性，用于反射，定义对象在分词时的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property,AllowMultiple=true)]
    public class SegmentAttribute:Attribute
    {
        //是否分词
        private bool _isSegment;
        //特性信息
        private string _message;

        public SegmentAttribute( bool blnIsSegment )
        {
            this._isSegment = blnIsSegment;
        }

        //是否分词
        public bool IsSegment
        {
            get { return this._isSegment; }
        }

        //特性信息
        public string Message
        {
            get{return this._message;}
            set { this._message = value; }
        }
    }
}
