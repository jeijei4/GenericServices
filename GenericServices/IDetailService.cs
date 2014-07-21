﻿using System;
using System.Linq.Expressions;
using GenericServices.Core;
using GenericServices.Services;

namespace GenericServices
{
    public interface IDetailService
    {
        /// <summary>
        /// This works out what sort of service is needed from the type provided
        /// and returns a single entry using the primary keys to find it.
        /// </summary>
        /// <typeparam name="T">The type of the data to output. 
        /// Type must be a type either an EF data class or one of the EfGenericDto's</typeparam>
        /// <param name="keys">The keys must be given in the same order as entity framework has them</param>
        /// <returns>Data class as read from database (not tracked)</returns>
        T GetDetail<T>(params object[] keys) where T : class, new();
    }

    public interface IDetailService<TData> where TData : class
    {
        /// <summary>
        /// This gets a single entry using the lambda expression as a where part
        /// </summary>
        /// <param name="whereExpression">Should be a 'where' expression that returns one item</param>
        /// <returns>Data class as read from database (not tracked)</returns>
        TData GetDetailUsingWhere(Expression<Func<TData, bool>> whereExpression);

        /// <summary>
        /// This finds an entry using the primary key(s) in the data
        /// </summary>
        /// <param name="keys">The keys must be given in the same order as entity framework has them</param>
        /// <returns>Data class as read from database (not tracked)</returns>
        TData GetDetail(params object[] keys);
    }

    public interface IDetailService<TData, out TDto>
        where TData : class
        where TDto : EfGenericDto<TData, TDto>
    {
        /// <summary>
        /// This gets a single entry using the lambda expression as a where part
        /// </summary>
        /// <param name="whereExpression">Should be a 'where' expression that returns one item</param>
        /// <returns>TDto type with properties copyed over</returns>
        TDto GetDetailUsingWhere(Expression<Func<TData, bool>> whereExpression);

        /// <summary>
        /// This finds an entry using the primary key(s) in the data
        /// </summary>
        /// <param name="keys">The keys must be given in the same order as entity framework has them</param>
        /// <returns>TDto type with properties copied over</returns>
        TDto GetDetail(params object[] keys);
    }

}