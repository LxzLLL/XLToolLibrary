using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XLToolLibrary.Utilities.FullSearch
{
    /// <summary>
    /// 作者：Arvin
    /// 日期：2015/9/15 10:04:33
    /// 描述：用于对实体的封装及索引的操作，目前暂未考虑对索引进行分组（例如一个表一个索引分组）
    /// 规则：每个文档，必须以ID为主键（32位UUID）
    /// </summary>
    public class FullTextOperation
    {


        #region 分词
        /// <summary>
        /// 返回分词结果
        /// </summary>
        /// <param name="keyword">关键词</param>
        /// <returns></returns>
        public static string Token(string keyword)
        {
            return PanGuLuceneHelper.instance.Token(keyword);
        }
        #endregion


        #region 创建索引
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="datalist"></param>
        /// <param name="dictBoost">权重设置，Dictionary<string,float>类型，如果不设置可以为null</param>
        /// <returns></returns>
        public static bool CreateIndex<T>(List<T> datalist, Dictionary<string, float> dictBoost)
        {
            return PanGuLuceneHelper.instance.CreateIndex<T>(datalist, dictBoost);
        }
        #endregion
        
        #region 更新索引
        /// <summary>
        /// 按ID更新索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="data">要更新的对象</param>
        /// <param name="dictBoost">要设置的权重</param>
        /// <returns></returns>
        public static bool Update<T>(string id, T data, Dictionary<string, float> dictBoost)
        {
            return PanGuLuceneHelper.instance.Update<T>(id, data, dictBoost);
        }
        #endregion 

        #region 检索索引
        /// <summary>
        /// 在传递的字段数组中查询数据，获取1000条数据
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns>返回</returns>
        public static List<T> Search<T>(string keyword, string[] fields, T objType)
        {
            return PanGuLuceneHelper.instance.Search<T>(keyword, fields, objType);
        }
        /// <summary>
        /// 在不同的类型下再根据检索字段中查询数据(分页)
        /// </summary>
        /// <param name="dType">分类,传空值查询全部，用于精确查找，例如类型，ID等，key为字段名，value为值</param>
        /// <param name="keyword">用于模糊查找的关键词</param>
        /// <param name="fields">需要检索的字段数组，如果为空，则检索有SegmentAttribute标记的属性，为null的话则不分词检索</param>
        /// <param name="objType">需要的对象</param>
        /// <param name="PageIndex">页数</param>
        /// <param name="PageSize">页大小</param>
        /// <param name="TotalCount">返回的数据总数</param>
        /// <param name="iTypeClause">分类之间的运算关系,三个值：0：must，且；1：must not，非；2：should，或；默认为0</param>
        /// <returns></returns>
        public static List<T> Search<T>(Dictionary<string, string> dType, string keyword, string[] fields, T objType, int PageIndex, int PageSize, out int TotalCount, int iTypeClause=0)
        {
            List<T> rtnList = new List<T>();
            TotalCount = 0;
            //fields为null时，直接构造空的数组，null时，不对分词的字段检索
            if (fields == null)
            {
                fields = new string[] { };
            }
            //fields为空时，检索有SegmentAttribute标记的属性，为空时，对所有字段进行检索
            else if(fields.Length<=0)
            {
                fields = GetSegmentAttributeProperty<T>(objType,true);
            }

           rtnList = PanGuLuceneHelper.instance.Search<T>(dType, keyword, fields, objType, PageIndex, PageSize, out TotalCount, iTypeClause);
            return rtnList;
        }

        #endregion

        #region 删除索引

        /// <summary>
        /// 删除索引数据（根据id）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool Delete(string id)
        {
            return PanGuLuceneHelper.instance.Delete(id);
        }

        /// <summary>
        /// 删除全部索引数据
        /// </summary>
        /// <returns></returns>
        public static bool DeleteAll()
        {
            return PanGuLuceneHelper.instance.DeleteAll();
        }

        #endregion


        #region 辅助函数
        /// <summary>
        /// 获取对象中拥有的SegmentAttribute标记的属性名称数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objType"></param>
        /// <param name="blnFlag">标示要获取分词的还是不分词的字段，true：标示分词；false：标示不分词</param>
        /// <returns></returns>
        public static string[] GetSegmentAttributeProperty<T>(T objType,bool blnFlag)
        {
            List<string> strList = new List<string>();
            if (objType != null)
            {
                Type t = objType.GetType();
                PropertyInfo[] pInfos = t.GetProperties();
                foreach (PropertyInfo p in pInfos)
                {
                    //获取属性的自定义特性
                    Object[] objAttribute = p.GetCustomAttributes(false);
                    if (objAttribute.Length <= 0) continue;
                    //由于属性只有一个特性，则使用First
                    //如果特性标记IsSegment为true，则表示是分词的属性，则将此属性名写入数组
                    bool blnIsSegment = ((SegmentAttribute)objAttribute.First()).IsSegment;
                    //需要分词，且标识也是分词，或者 需要不分词，且标识也是不分词
                    if(blnFlag==blnIsSegment)
                    {
                        strList.Add(p.Name);
                    }
                }
            }
            return strList.ToArray();
        }

        #endregion

    }
}
