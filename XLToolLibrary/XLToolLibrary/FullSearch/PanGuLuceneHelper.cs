using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using System.Reflection;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;


/// <summary>
/// 作者：Arvin
/// 日期：2015/9/11 9:54:17
/// 描述：基于Lucene.net的全文检索
/// 使用范围：目前暂未考虑对索引进行分组（例如一个表一个索引分组）
/// </summary>
namespace XLToolLibrary.Utilities.FullSearch
{
    /// <summary>
    /// 盘古分词在lucene.net中的使用帮助类
    /// 调用PanGuLuceneHelper.instance
    /// </summary>
    public class PanGuLuceneHelper
    {
        private PanGuLuceneHelper() 
        {            
        }

        #region 单一实例
        private static PanGuLuceneHelper _instance = null;
        /// <summary>
        /// 单一实例
        /// </summary>
        public static PanGuLuceneHelper instance
        {
            get
            {
                if (_instance == null) _instance = new PanGuLuceneHelper();
                return _instance;
            }
        }
        #endregion

        #region 分词测试
        /// <summary>
        /// 分词测试
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public string Token(string keyword)
        {
            string ret = "";
            System.IO.StringReader reader = new System.IO.StringReader(keyword);
            Lucene.Net.Analysis.TokenStream ts = analyzer.TokenStream(keyword, reader);
            Lucene.Net.Analysis.Token token = ts.Next();
            while (token != null)
            {
                ret += " " + token.TermText();
                token = ts.Next();
            }
            ts.CloneAttributes();
            reader.Close();
            analyzer.Close();
            return ret;
        }
        #endregion

        #region 创建索引
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="datalist"></param>
        /// <param name="dictBoost">权重设置，Dictionary<string,float>类型，如果不设置可以为null</param>
        /// <returns></returns>
        public bool CreateIndex<T>(List<T> datalist, Dictionary<string, float> dictBoost)
        {
            IndexWriter writer = null;
            if(IndexWriter.IsLocked(directory_luce))
            {
                IndexWriter.Unlock(directory_luce);
            }
            try
            {
                writer = new IndexWriter(directory_luce, analyzer, false, IndexWriter.MaxFieldLength.LIMITED);//false表示追加（true表示删除之前的重新写入）
            }
            catch
            {
                writer = new IndexWriter(directory_luce, analyzer, true, IndexWriter.MaxFieldLength.LIMITED);//false表示追加（true表示删除之前的重新写入）
            }
            foreach (T data in datalist)
            {
                CreateIndex(writer, data, dictBoost);
            }
            writer.Optimize();
            writer.Close();
            return true;
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="data"></param>
        /// <param name="dictBoost">权重设置，Dictionary<string,float>类型，如果不设置可以为null</param>
        /// <returns></returns>
        public bool CreateIndex<T>(IndexWriter writer, T data,Dictionary<string,float> dictBoost)
        {
            if (data == null) return false;
            try
            {
                Document doc = GetDoc<T>(data, dictBoost);
                writer.AddDocument(doc);
            }
            catch (System.IO.FileNotFoundException fnfe)
            {
                throw fnfe;
            }
            return true;
        }
        #endregion


        #region 获取document文档
        /// <summary>
        /// 根据给定的对象和字段权重设置，获取doc文档
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="data">类型实例</param>
        /// <param name="dictBoost">权重设置，Dictionary<string,float>类型，如果不设置可以为空</param>
        /// <returns></returns>
        protected  Document GetDoc<T>(T data, Dictionary<string, float> dictBoost)
        {
            Document doc = new Document();
            Type type = data.GetType();//assembly.GetType("Reflect_test.PurchaseOrderHeadManageModel", true, true); //命名空间名称 + 类名    

            //创建类的实例    
            //object obj = Activator.CreateInstance(type, true);  
            //获取公共属性    
            PropertyInfo[] Propertys = type.GetProperties();
            for (int i = 0; i < Propertys.Length; i++)
            {
                //Propertys[i].SetValue(Propertys[i], i, null); //设置值
                PropertyInfo pi = Propertys[i];
                string name = pi.Name;
                object objval = pi.GetValue(data, null);
                string value = objval == null ? "" : objval.ToString(); //值

                //获取属性的自定义特性
                object[] objsAttribute = pi.GetCustomAttributes(false);
                if (objsAttribute.Length <= 0)
                {
                    //doc.Add(new Field(name, value, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    continue;
                }
                else
                {
                    //由于只有一个特性，则使用First
                    SegmentAttribute sa = (SegmentAttribute)objsAttribute.First();
                    if (sa.IsSegment)//在特性标记为真的情况下，则对此进行分词
                    {
                        //Field field = new Field(name, value, Field.Store.YES, Field.Index.ANALYZED);
                        Field field = SetFieldBoost(new Field(name, value, Field.Store.YES, Field.Index.ANALYZED), dictBoost);
                        doc.Add(new Field(name, value, Field.Store.YES, Field.Index.ANALYZED));//分词
                    }
                    else       //其他情况下不分词，否则如果是模糊搜索和删除，会出现混乱
                    {
                        doc.Add(new Field(name, value, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }
                }
            }
            return doc;
        }
        #endregion



        #region 对字段设置权重
        /// <summary>
        /// 设置字段权重
        /// </summary>
        /// <param name="field">Field字段</param>
        /// <param name="dictBoost">权重字典，调用者配置</param>
        /// <returns></returns>
        public Field SetFieldBoost(Field field, Dictionary<string, float> dictBoost)
        {
            Field f = field;
            float boostValue;
            if ( dictBoost!=null && dictBoost.TryGetValue(f.Name(), out boostValue))
            {
                f.SetBoost(boostValue);
            }
            return f;
        }
        #endregion


        #region 在传递的字段数组中查询数据
        /// <summary>
        /// 在传递的字段数组中查询数据
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns>返回</returns>
        public List<T> Search<T>(string keyword,string[] fields,T objType)
        {

            //fields = new string[]{ "title", "content" };//查询字段
            //Stopwatch st = new Stopwatch();  
            //st.Start();
            QueryParser parser = null;// new QueryParser(Lucene.Net.Util.Version.LUCENE_30, field, analyzer);//一个字段查询
            parser = new MultiFieldQueryParser(version,fields, analyzer);//多个字段查询
            Query query = parser.Parse(keyword);
            int n = 1000;
            IndexSearcher searcher = new IndexSearcher(directory_luce, true);//true-表示只读
            List<T> list = new List<T>();
            TopDocs docs = searcher.Search(query, (Filter)null, n);
            if (docs == null || docs.totalHits == 0)
            {
                return list;
            }
            else
            {
                PanGu.HighLight.SimpleHTMLFormatter simpleHTMLFormatter = new PanGu.HighLight.SimpleHTMLFormatter("<font style='color:red'>", "</font>");
                PanGu.HighLight.Highlighter highlighter = new PanGu.HighLight.Highlighter(simpleHTMLFormatter, new PanGu.Segment());
                highlighter.FragmentSize = 50;
                foreach (ScoreDoc sd in docs.scoreDocs)//遍历搜索到的结果
                {
                    try
                    {
                        //获取对象
                        Document doc = searcher.Doc(sd.doc);
                        Type t = objType.GetType();
                        T obj = (T)Activator.CreateInstance(t, true);
                        PropertyInfo[] propertyList = t.GetProperties();
                        //设置对象，并填充搜索到的数据至对象
                        foreach (PropertyInfo property in propertyList)
                        {
                            string strPName = property.Name;
                            string strValue = doc.Get(strPName);
                            if (fields!=null&&fields.Contains(strPName))
                            {
                                string strHighterValue = highlighter.GetBestFragment(keyword, strValue);
                                strValue = strHighterValue == "" ? strValue : strHighterValue;
                            }
                            property.SetValue(obj, strValue, null);
                        }
                        list.Add(obj);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                return list;
            }
            //st.Stop();
            //Response.Write("查询时间：" + st.ElapsedMilliseconds + " 毫秒<br/>");
        }
        #endregion

        #region 在不同的分类下再根据检索字段中查询数据(分页)
        /// <summary>
        /// 在不同的类型下再根据检索字段中查询数据(分页)
        /// </summary>
        /// <param name="dType">分类,传空值查询全部，用于精确查找，例如类型等，key为字段名，value为值</param>
        /// <param name="iTypeClause">分类之间的运算关系,三个值：0：must，且；1：must not，非；2：should，或；默认为0</param>
        /// <param name="keyword">用于模糊查找的关键词</param>
        /// <param name="objType">需要的对象</param>
        /// <param name="PageIndex">页数</param>
        /// <param name="PageSize">页大小</param>
        /// <param name="TotalCount">返回的数据总数</param>
        /// <returns></returns>
        public List<T> Search<T>(Dictionary<string, string> dType, string keyword, string[] fields, T objType, int PageIndex, int PageSize, out int TotalCount, int iTypeClause)
        {
            if (PageIndex < 1) PageIndex = 1;
            //Stopwatch st = new Stopwatch();
            //st.Start();
            BooleanQuery bq = new BooleanQuery();
            //对类型进行与运算
            if (dType!=null&&dType.Count>0)
            {
                foreach(KeyValuePair<string,string> kvp in dType)
                {
                    //使用术语查询
                    Query tq = new TermQuery(new Term(kvp.Key, kvp.Value));
                    BooleanClause.Occur occur = BooleanClause.Occur.MUST;
                    switch(iTypeClause)
                    {
                        case 0: occur = BooleanClause.Occur.MUST;
                            break;
                        case 1: occur = BooleanClause.Occur.MUST_NOT;
                            break;
                        case 2: occur = BooleanClause.Occur.SHOULD;
                            break;
                        default: occur = BooleanClause.Occur.MUST;
                            break;
                    }

                    bq.Add(tq, occur);//与运算
                }
            }
            //对关键字进行或运算
            if (keyword != "")
            {
                QueryParser parser = null;// new QueryParser(version, field, analyzer);//一个字段查询
                parser = new MultiFieldQueryParser(version,fields, analyzer);//多个字段查询
                Query queryKeyword = parser.Parse(keyword);
                bq.Add(queryKeyword, BooleanClause.Occur.SHOULD);//或运算
            }
            List<T> list = new List<T>();

            TopScoreDocCollector collector = TopScoreDocCollector.create(PageIndex * PageSize, true);
            IndexSearcher searcher = new IndexSearcher(directory_luce, true);//true-表示只读
            searcher.Search(bq, collector);
            if (collector == null || collector.GetTotalHits() == 0)
            {
                TotalCount = 0;
                return list;
            }
            else
            {
                int start = PageSize * (PageIndex - 1);
                //结束数
                int limit = PageSize;
                ScoreDoc[] hits = collector.TopDocs(start, limit).scoreDocs;
                TotalCount = collector.GetTotalHits();

                PanGu.HighLight.SimpleHTMLFormatter simpleHTMLFormatter = new PanGu.HighLight.SimpleHTMLFormatter("<font style='color:red'>", "</font>");
                PanGu.HighLight.Highlighter highlighter = new PanGu.HighLight.Highlighter(simpleHTMLFormatter, new PanGu.Segment());
                highlighter.FragmentSize = 50;
                //遍历搜索到的结果
                foreach (ScoreDoc sd in hits)
                {
                    try
                    {
                        //获取对象
                        Document doc = searcher.Doc(sd.doc);
                        Type t = objType.GetType();
                        T obj = (T)Activator.CreateInstance(t, true);
                        PropertyInfo[] propertyList = t.GetProperties();
                        //设置对象，并填充搜索到的数据至对象
                        foreach (PropertyInfo property in propertyList)
                        {
                            string strPName = property.Name;
                            string strValue = doc.Get(strPName);
                            if (fields != null && fields.Contains(strPName))
                            {
                                string strHighterValue = highlighter.GetBestFragment(keyword, strValue);
                                strValue = strHighterValue == "" ? strValue : strHighterValue;
                            }
                            property.SetValue(obj, strValue, null);
                        }
                        list.Add(obj);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                return list;
            }
            //st.Stop();
            //Response.Write("查询时间：" + st.ElapsedMilliseconds + " 毫秒<br/>");

        }
        #endregion

        #region 删除索引数据（根据id）
        /// <summary>
        /// 删除索引数据（根据id）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(string id)
        {
            bool IsSuccess = false;
            Term term = new Term("ID", id);

            IndexWriter writer = GetIndexWriter();
            try
            {
                writer.DeleteDocuments(term); // writer.DeleteDocuments(term)或者writer.DeleteDocuments(query);
                writer.Commit();
                //writer.Optimize();//
                IsSuccess = writer.HasDeletions();
            }
            catch
            {
                IsSuccess = false;
            }
            finally
            {
                writer.Close();
            }
            return IsSuccess;
        }
        #endregion

        #region 删除全部索引数据
        /// <summary>
        /// 删除全部索引数据
        /// </summary>
        /// <returns></returns>
        public bool DeleteAll()
        {
            bool IsSuccess = true;
            IndexWriter writer = GetIndexWriter();
            try
            {
                writer.DeleteAll();
                writer.Commit();
                //writer.Optimize();   //优化索引
                IsSuccess = writer.HasDeletions();
            }
            catch
            {
                IsSuccess = false;
            }
            finally
            {
                writer.Close();
            }
            return IsSuccess;
        }
        #endregion


        #region 按ID更新数据
        /// <summary>
        /// 按ID更新索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="dictBoost"></param>
        /// <returns></returns>
        public bool Update<T>( string id,T data,Dictionary<string,float> dictBoost)
        {
            bool IsSucess = false;
            Term term = new Term("ID", id);
            Document doc = GetDoc<T>(data, dictBoost);

            IndexWriter writer = GetIndexWriter();
            try
            {
                writer.UpdateDocument(term, doc);
                IsSucess = true;
                writer.Optimize();
                writer.Commit();
            }
            catch
            {
                IsSucess = false;
            }
            finally
            {
                writer.Close();
            }
            return IsSucess;

        }
        #endregion

        #region  获取IndexWriter
        public IndexWriter GetIndexWriter()
        {
            if(IndexWriter.IsLocked(directory_luce))
            {
                IndexWriter.Unlock(directory_luce);
            }
            return new IndexWriter(directory_luce, analyzer, false, IndexWriter.MaxFieldLength.LIMITED);
        }
        #endregion


        #region directory_luce
        private Lucene.Net.Store.Directory _directory_luce = null;
        /// <summary>
        /// Lucene.Net的目录-参数
        /// </summary>
        public Lucene.Net.Store.Directory directory_luce
        {
            get
            {
                if (_directory_luce == null) _directory_luce = Lucene.Net.Store.FSDirectory.Open(directory);
                return _directory_luce;
            }
        } 
        #endregion

        #region directory
        private System.IO.DirectoryInfo _directory = null;
        /// <summary>
        /// 索引在硬盘上的目录
        /// </summary>
        public System.IO.DirectoryInfo directory
        {
            get
            {
                if (_directory == null)
                {
                    string dirPath = AppDomain.CurrentDomain.BaseDirectory + "\\EMFiles\\LuceneIndex";
                    if (System.IO.Directory.Exists(dirPath) == false) _directory = System.IO.Directory.CreateDirectory(dirPath);
                    else _directory = new System.IO.DirectoryInfo(dirPath);
                }
                return _directory;
            }
        } 
        #endregion

        #region analyzer
        private Analyzer _analyzer = null;
        /// <summary>
        /// 分析器
        /// </summary>
        public Analyzer analyzer
        {
            get
            {
                //if (_analyzer == null)
                {
                    _analyzer = new Lucene.Net.Analysis.PanGu.PanGuAnalyzer();//
                }
                return _analyzer;
            }
        } 
        #endregion

        #region version
        private static Lucene.Net.Util.Version _version = Lucene.Net.Util.Version.LUCENE_29;
        /// <summary>
        /// 版本号枚举类
        /// </summary>
        public Lucene.Net.Util.Version version
        {
            get
            {
                return _version;
            }
        }
        #endregion
    }

}