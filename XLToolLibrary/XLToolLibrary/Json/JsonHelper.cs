using System;
using System.Text;
using Newtonsoft.Json;

namespace XLToolLibrary.Utilities
{
    public class JsonHelper
    {
        /// <summary>
        /// 从对象转为json字符串
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static string ConvertObject2JsonString(object obj)
        {
            string sJson = string.Empty;
            if(obj == null)
            {
                return sJson;
            }
            return JsonConvert.SerializeObject( obj );
        }

        /// <summary>
        /// 从对象转为json字符串，并添加前缀，比如增加{"data":[json]}中的data前缀，用于部分jqueryUI数据生成
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="sPrefix">前缀</param>
        /// <returns></returns>
        public static string ConvertObject2JsonStringByPrefix(object obj,string sPrefix)
        {
            string sJson = string.Empty;
            if ( obj == null )
            {
                return sJson;
            }
            if ( string.IsNullOrEmpty( sPrefix ) )
            {
                return ConvertObject2JsonString( obj ) ;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append( "{\""+ sPrefix + "\":" );
            sb.AppendFormat( "{0}", ConvertObject2JsonString( obj ) );
            sb.Append( "}" );
            return sb.ToString();
        }
    }
}
