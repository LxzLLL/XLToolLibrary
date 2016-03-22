﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Text;

namespace XLToolLibrary.Utilities
{
    /// <summary>
    /// 响应结果类
    /// </summary>
    public class ResponseResult
    {
        /// <summary>
        /// 状态(0-default, 1-success, -1-error)
        /// </summary>
        public int Status
        {
            get;
            set;
        }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// 新ID值
        /// </summary>
        public dynamic NewID
        {
            get;
            set;
        }

        /// <summary>
        /// 消息额外数据
        /// </summary>
        public object ExtraData
        {
            get;
            set;
        }

        /// <summary>
        /// 响应消息封装类
        /// </summary>
        /// <param name="Status">状态:1-成功；0-缺省; -1失败</param>
        /// <param name="Message">消息内容</param>
        /// <returns></returns>
        public static ResponseResult Default(string message = null)
        {
            var result = new ResponseResult();
            result.Status = 0;
            result.Message = message;

            return result;
        }

        /// <summary>
        /// 响应消息封装类
        /// </summary>
        /// <param name="Status">状态:1-成功；0-缺省; -1失败</param>
        /// <param name="Message">消息内容</param>
        /// <returns></returns>
        public static ResponseResult Success(string message = null)
        {
            var result = new ResponseResult();
            result.Status = 1;
            result.Message = message;

            return result;
        }

        /// <summary>
        /// 响应消息封装类，用于插入操作，返回新ID
        /// </summary>
        /// <param name="Status">状态:1-成功; 0-缺省; -1失败</param>
        /// <param name="Message">消息内容</param>
        /// <returns></returns>
        public static ResponseResult Success(dynamic newId, string message = null)
        {
            var result = new ResponseResult();
            result.NewID = newId;
            result.Status = 1;
            result.Message = message;

            return result;
        }

        /// <summary>
        /// 响应消息封装类
        /// </summary>
        /// <param name="Status">状态:1-成功；0-缺省; -1失败</param>
        /// <param name="Message">消息内容</param>
        /// <returns></returns>
        public static ResponseResult Error(string errorMessage)
        {
            var result = new ResponseResult();
            result.Status = -1;
            result.Message = errorMessage;
            return result;
        }

        /// <summary>
        /// 解析返回结果中的状态标志位
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool Parse(string message)
        {
            var result = JsonSerializer.DeserializeFromString<ResponseResult>(message);
            var success = result.Status == 1;

            return success;
        }

        public static ResponseResult Deserialize(string message)
        {
            var result = JsonSerializer.DeserializeFromString<ResponseResult>(message);
            return result;
        }
    }

    /// <summary>
    /// 响应消息类(泛型)
    /// </summary>
    public class ResponseResult<T> where T : class
    {
        /// <summary>
        /// 状态
        /// </summary>
        public int Status
        {
            get;
            set;
        }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// 新GUID值
        /// </summary>
        public dynamic NewID
        {
            get;
            set;
        }

        /// <summary>
        /// 业务实体
        /// </summary>
        public T Entity
        {
            get;
            set;
        }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages
        {
            get;
            set;
        }

        /// <summary>
        /// 总行数
        /// </summary>
        public int TotalRowsCount
        {
            get;
            set;
        }

        /// <summary>
        /// 响应消息封装类
        /// </summary>
        /// <param name="Status">状态:1-成功; 0-缺省; -1失败</param>
        /// <param name="Message">消息内容</param>
        /// <returns></returns>
        public static ResponseResult<T> Default()
        {
            var result = new ResponseResult<T>();
            result.Entity = null;
            result.Status = 0;
            result.Message = string.Empty;

            return result;
        }

        /// <summary>
        /// 响应消息封装类
        /// </summary>
        /// <param name="Status">状态:1-成功; 0-缺省; -1失败</param>
        /// <param name="Message">消息内容</param>
        /// <returns></returns>
        public static ResponseResult<T> Success(T t, string message = null)
        {
            var result = new ResponseResult<T>();
            result.Entity = t;
            result.Status = 1;
            result.Message = message;

            return result;
        }

        /// <summary>
        /// 响应消息封装类，用于插入操作，返回新ID
        /// </summary>
        /// <param name="Status">状态:1-成功; 0-缺省; -1失败</param>
        /// <param name="Message">消息内容</param>
        /// <returns></returns>
        public static ResponseResult<T> Success(int newId, string message = null)
        {
            var result = new ResponseResult<T>();
            result.NewID = newId;
            result.Status = 1;
            result.Message = message;
            return result;
        }
        /// <summary>
        /// Http 响应消息封装类
        /// </summary>
        /// <param name="Status">状态:1-成功; 0-缺省; -1失败</param>
        /// <param name="Message">消息内容</param>
        /// <returns></returns>
        public static ResponseResult<T> Error(string message = null)
        {
            var result = new ResponseResult<T>();
            result.Entity = null;
            result.Status = -1;
            result.Message = message;

            return result;
        }

        /// <summary>
        /// Http 响应消息反序列化类
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static ResponseResult<T> Deserialize(string message)
        {
            var response = JsonSerializer.DeserializeFromString<ResponseResult<T>>(message);
            return response;
        }
    }
}
