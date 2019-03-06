﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerShapedQueryOptimizingExpressionVisitors : RelationalShapedQueryOptimizingExpressionVisitors
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerShapedQueryOptimizingExpressionVisitors(QueryCompilationContext2 queryCompilationContext,
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory)
            : base(queryCompilationContext)
        {
            _typeMappingSource = typeMappingSource;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public override IEnumerable<ExpressionVisitor> GetVisitors()
        {
            foreach (var visitor in base.GetVisitors())
            {
                yield return visitor;
            }

            yield return new SearchConditionConvertingExpressionVisitor(_typeMappingSource, _sqlExpressionFactory);
        }
    }
}