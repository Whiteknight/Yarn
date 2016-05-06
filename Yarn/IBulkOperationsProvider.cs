﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Yarn
{
    public interface IBulkOperationsProvider
    {
        IEnumerable<T> GetById<T, TKey>(IEnumerable<TKey> ids) where T : class;

        long Insert<T>(IEnumerable<T> entities) where T : class;

        long Update<T>(Expression<Func<T, bool>> criteria, Expression<Func<T, T>> update) where T : class;

        long Update<T>(params BulkUpdateOperation<T>[] bulkOperations) where T : class;

        long Delete<T>(IEnumerable<T> entities) where T : class;

        long Delete<T, TKey>(IEnumerable<TKey> ids) where T : class;

        long Delete<T>(params Expression<Func<T, bool>>[] criteria) where T : class;

        long Delete<T>(params ISpecification<T>[] criteria) where T : class;
    }
}
