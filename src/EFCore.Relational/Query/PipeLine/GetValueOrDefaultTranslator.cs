// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class GetValueOrDefaultTranslator : IMethodCallTranslator
    {
        private readonly ITypeMappingApplyingExpressionVisitor _typeMappingApplyingExpressionVisitor;

        public GetValueOrDefaultTranslator(ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            _typeMappingApplyingExpressionVisitor = typeMappingApplyingExpressionVisitor;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (method.Name == nameof(Nullable<int>.GetValueOrDefault)
                && method.ReturnType.IsNumeric())
            {
                return _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                    new SqlBinaryExpression(
                        ExpressionType.Coalesce,
                        instance,
                        arguments.Count == 0
                            ? GetDefaultConstant(method.ReturnType)
                            : arguments[0],
                        method.ReturnType,
                        null),
                    instance.TypeMapping);
            }

            return null;
        }

        private SqlConstantExpression GetDefaultConstant(Type type)
        {
            return (SqlConstantExpression)_generateDefaultValueConstantMethod
                .MakeGenericMethod(type).Invoke(null, Array.Empty<object>());
        }

        private static readonly MethodInfo _generateDefaultValueConstantMethod =
            typeof(GetValueOrDefaultTranslator).GetTypeInfo().GetDeclaredMethod(nameof(GenerateDefaultValueConstant));

        private static SqlConstantExpression GenerateDefaultValueConstant<TDefault>()
            => new SqlConstantExpression(Expression.Constant(default(TDefault)), null);
    }
}
