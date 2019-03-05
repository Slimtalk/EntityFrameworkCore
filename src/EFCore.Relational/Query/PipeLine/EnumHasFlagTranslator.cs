// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class EnumHasFlagTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(Enum).GetRuntimeMethod(nameof(Enum.HasFlag), new[] { typeof(Enum) });

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ITypeMappingApplyingExpressionVisitor _typeMappingApplyingExpressionVisitor;

        public EnumHasFlagTranslator(
            IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            _typeMappingSource = typeMappingSource;
            _typeMappingApplyingExpressionVisitor = typeMappingApplyingExpressionVisitor;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (Equals(method, _methodInfo))
            {
                var argument = arguments[0];
                var typeMapping = ExpressionExtensions.InferTypeMapping(instance, argument);
                instance = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(instance, typeMapping);
                argument = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(argument, typeMapping);

                if (instance.Type.UnwrapNullableType() != argument.Type.UnwrapNullableType())
                {
                    return null;
                }

                return new SqlBinaryExpression(
                    ExpressionType.Equal,
                    new SqlBinaryExpression(
                        ExpressionType.And,
                        instance,
                        argument,
                        instance.Type,
                        typeMapping),
                    argument,
                    typeof(bool),
                    _typeMappingSource.FindMapping(typeof(bool)));
            }

            return null;
        }
    }
}
