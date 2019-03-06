// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public interface ISqlExpressionFactory
    {
        SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, RelationalTypeMapping typeMapping);
        SqlExpression ApplyDefaultTypeMapping(SqlExpression sqlExpression);
        CaseExpression Case(SqlExpression operand, params CaseWhenClause[] whenClauses);
        CaseExpression Case(IReadOnlyList<CaseWhenClause> whenClauses, SqlExpression elseResult);
        ExistsExpression Exists(SelectExpression subquery, bool negated);
        SqlFunctionExpression SqlFunction(
            string functionName, IEnumerable<SqlExpression> arguments, Type returnType, RelationalTypeMapping typeMapping);
        SqlFunctionExpression SqlFunction(
            string schema, string functionName, IEnumerable<SqlExpression> arguments, Type returnType, RelationalTypeMapping typeMapping);
        SqlFunctionExpression SqlFunction(
            SqlExpression instance, string functionName, IEnumerable<SqlExpression> arguments, Type returnType, RelationalTypeMapping typeMapping);
        SqlFunctionExpression SqlFunction(
            string functionName, bool niladic, Type returnType, RelationalTypeMapping typeMapping);
        SqlFunctionExpression SqlFunction(
            string schema, string functionName, bool niladic, Type returnType, RelationalTypeMapping typeMapping);
        SqlFunctionExpression SqlFunction(
            SqlExpression instance, string functionName, bool niladic, Type returnType, RelationalTypeMapping typeMapping);
        SqlNullExpression IsNull(SqlExpression operand);
        SqlNullExpression IsNotNull(SqlExpression operand);
        SqlConstantExpression Constant(object value);
        SqlBinaryExpression MakeBinary(ExpressionType operatorType, SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping);

        SqlBinaryExpression Equal(SqlExpression left, SqlExpression right);

        SqlUnaryExpression Convert(SqlExpression operand, Type type, RelationalTypeMapping typeMapping);
    }
}
