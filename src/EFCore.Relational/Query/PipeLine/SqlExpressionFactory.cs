// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class SqlExpressionFactory : ISqlExpressionFactory
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly RelationalTypeMapping _boolTypeMapping;

        public SqlExpressionFactory(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
            _boolTypeMapping = _typeMappingSource.FindMapping(typeof(bool));
        }

        public SqlExpression ApplyDefaultTypeMapping(SqlExpression sqlExpression)
        {
            return ApplyTypeMapping(sqlExpression, _typeMappingSource.FindMapping(sqlExpression.Type));
        }

        public SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, RelationalTypeMapping typeMapping)
        {
            if (sqlExpression == null
                || sqlExpression.TypeMapping != null
                || typeMapping == null)
            {
                return sqlExpression;
            }

            switch (sqlExpression)
            {
                case CaseExpression caseExpression:
                    return ApplyTypeMappingOnCase(caseExpression, typeMapping);

                //case LikeExpression likeExpression:
                //    return ApplyTypeMappingOnLike(likeExpression, typeMapping);

                case SqlBinaryExpression sqlBinaryExpression:
                    return ApplyTypeMappingOnSqlBinary(sqlBinaryExpression, typeMapping);

                case SqlUnaryExpression sqlUnaryExpression:
                    return ApplyTypeMappingOnSqlUnary(sqlUnaryExpression, typeMapping);

                case SqlConstantExpression sqlConstantExpression:
                    return sqlConstantExpression.ApplyTypeMapping(typeMapping);

                //case SqlFragmentExpression sqlFragmentExpression:
                //    return sqlFragmentExpression;

                //case SqlFunctionExpression sqlFunctionExpression:
                //    return ApplyTypeMappingOnSqlFunction(sqlFunctionExpression, typeMapping);

                case SqlParameterExpression sqlParameterExpression:
                    return sqlParameterExpression.ApplyTypeMapping(typeMapping);

                    //default:
                    //    return ApplyTypeMappingOnExtension(expression, typeMapping);

            }

            return null;
        }

        protected virtual SqlExpression ApplyTypeMappingOnCase(
            CaseExpression caseExpression, RelationalTypeMapping typeMapping)
        {
            var whenClauses = new List<CaseWhenClause>();

            foreach (var caseWhenClause in caseExpression.WhenClauses)
            {
                whenClauses.Add(
                    new CaseWhenClause(
                        ApplyTypeMapping(caseWhenClause.Test, _boolTypeMapping),
                        ApplyTypeMapping(caseWhenClause.Result, typeMapping)));
            }

            var elseResult = ApplyTypeMapping(caseExpression.ElseResult, typeMapping);

            return new CaseExpression(
                whenClauses,
                elseResult);
        }

        protected virtual SqlExpression ApplyTypeMappingOnSqlUnary(
            SqlUnaryExpression sqlUnaryExpression, RelationalTypeMapping typeMapping)
        {
            return new SqlUnaryExpression(
                sqlUnaryExpression.OperatorType,
                ApplyDefaultTypeMapping(sqlUnaryExpression.Operand),
                sqlUnaryExpression.Type,
                typeMapping);
        }

        protected virtual SqlExpression ApplyTypeMappingOnSqlBinary(
            SqlBinaryExpression sqlBinaryExpression, RelationalTypeMapping typeMapping)
        {
            var left = sqlBinaryExpression.Left;
            var right = sqlBinaryExpression.Right;

            switch (sqlBinaryExpression.OperatorType)
            {
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    {
                        var inferredTypeMapping = InferTypeMappingForBinary(left, right);

                        left = ApplyTypeMapping(left, inferredTypeMapping);
                        right = ApplyTypeMapping(right, inferredTypeMapping);

                        return new SqlBinaryExpression(
                            sqlBinaryExpression.OperatorType,
                            left,
                            right,
                            typeof(bool),
                            _boolTypeMapping);
                    }

                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    {
                        left = ApplyTypeMapping(left, _boolTypeMapping);
                        right = ApplyTypeMapping(right, _boolTypeMapping);

                        return new SqlBinaryExpression(
                            sqlBinaryExpression.OperatorType,
                            left,
                            right,
                            typeof(bool),
                            _boolTypeMapping);
                    }

                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.Coalesce:
                    {
                        var inferredTypeMapping = typeMapping ?? InferTypeMappingForBinary(left, right);

                        left = ApplyTypeMapping(left, inferredTypeMapping);
                        right = ApplyTypeMapping(right, inferredTypeMapping);

                        return new SqlBinaryExpression(
                            sqlBinaryExpression.OperatorType,
                            left,
                            right,
                            left.Type,
                            inferredTypeMapping);
                    }

                case ExpressionType.And:
                case ExpressionType.Or:
                    return null;

                default:
                    return null;
            }
        }

        public CaseExpression Case(SqlExpression operand, params CaseWhenClause[] whenClauses)
        {
            //var operandTypeMapping = operand.TypeMapping;
            //if (operandTypeMapping == null)
            //{
            //    throw new InvalidCastException("Null TypeMapping");
            //}

            //var resultTypeMapping =

            //var typeMappedWhenClauses = new List<CaseWhenClause>();



            throw new NotImplementedException();
        }

        public CaseExpression Case(IReadOnlyList<CaseWhenClause> whenClauses, SqlExpression elseResult)
        {
            var typeMappedWhenClauses = new List<CaseWhenClause>();
            var resultTypeMapping = elseResult?.TypeMapping
                ?? whenClauses.Select(wc => wc.Result.TypeMapping).FirstOrDefault(t => t != null);
            foreach (var item in whenClauses)
            {
                typeMappedWhenClauses.Add(
                    new CaseWhenClause(
                        ApplyTypeMapping(item.Test, _boolTypeMapping),
                        ApplyTypeMapping(item.Result, resultTypeMapping)));
            }

            elseResult = ApplyTypeMapping(elseResult, resultTypeMapping);

            return new CaseExpression(typeMappedWhenClauses, elseResult);
        }

        public SqlConstantExpression Constant(object value)
        {
            return new SqlConstantExpression(Expression.Constant(value), null);
        }

        public ExistsExpression Exists(SelectExpression subquery, bool negated)
        {
            throw new NotImplementedException();
        }

        public SqlNullExpression IsNotNull(SqlExpression operand)
        {
            operand = ApplyDefaultTypeMapping(operand);

            return new SqlNullExpression(operand, true, _typeMappingSource.FindMapping(typeof(bool)));
        }

        public SqlNullExpression IsNull(SqlExpression operand)
        {
            operand = ApplyDefaultTypeMapping(operand);

            return new SqlNullExpression(operand, false, _typeMappingSource.FindMapping(typeof(bool)));
        }

        public SqlBinaryExpression MakeBinary(
            ExpressionType operatorType, SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping)
        {
            switch (operatorType)
            {
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    {
                        var inferredTypeMapping = InferTypeMappingForBinary(left, right);

                        left = ApplyTypeMapping(left, inferredTypeMapping);
                        right = ApplyTypeMapping(right, inferredTypeMapping);

                        return new SqlBinaryExpression(
                            operatorType,
                            left,
                            right,
                            typeof(bool),
                            _boolTypeMapping);
                    }

                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    {
                        left = ApplyTypeMapping(left, _boolTypeMapping);
                        right = ApplyTypeMapping(right, _boolTypeMapping);

                        return new SqlBinaryExpression(
                            operatorType,
                            left,
                            right,
                            typeof(bool),
                            _boolTypeMapping);
                    }

                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.Coalesce:
                    {
                        var inferredTypeMapping = typeMapping ?? InferTypeMappingForBinary(left, right);

                        left = ApplyTypeMapping(left, inferredTypeMapping);
                        right = ApplyTypeMapping(right, inferredTypeMapping);

                        return new SqlBinaryExpression(
                            operatorType,
                            left,
                            right,
                            left.Type,
                            inferredTypeMapping);
                    }

                case ExpressionType.And:
                case ExpressionType.Or:
                    return null;

                default:
                    return null;
            }
        }

        private RelationalTypeMapping InferTypeMappingForBinary(SqlExpression left, SqlExpression right)
        {
            var typeMapping = ExpressionExtensions.InferTypeMapping(left, right);

            if (typeMapping == null)
            {
                if (left is SqlUnaryExpression)
                {
                    typeMapping = _typeMappingSource.FindMapping(left.Type);
                }
                else if (right is SqlUnaryExpression)
                {
                    typeMapping = _typeMappingSource.FindMapping(right.Type);
                }
                else
                {
                    throw new InvalidOperationException("TypeMapping should not be null.");
                }
            }

            return typeMapping;
        }

        public SqlFunctionExpression SqlFunction(
            string functionName, IEnumerable<SqlExpression> arguments, Type returnType, RelationalTypeMapping typeMapping)
        {
            var typeMappedArguments = new List<SqlExpression>();

            foreach (var argument in arguments)
            {
                typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
            }

            return new SqlFunctionExpression(
                functionName,
                typeMappedArguments,
                returnType,
                typeMapping);
        }

        public SqlBinaryExpression Equal(SqlExpression left, SqlExpression right)
        {
            return MakeBinary(ExpressionType.Equal, left, right, null);
        }

        public SqlFunctionExpression SqlFunction(
            string schema, string functionName, IEnumerable<SqlExpression> arguments, Type returnType, RelationalTypeMapping typeMapping)
        {
            var typeMappedArguments = new List<SqlExpression>();

            foreach (var argument in arguments)
            {
                typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
            }

            return new SqlFunctionExpression(
                schema,
                functionName,
                typeMappedArguments,
                returnType,
                typeMapping);
        }

        public SqlFunctionExpression SqlFunction(
            SqlExpression instance, string functionName, IEnumerable<SqlExpression> arguments, Type returnType, RelationalTypeMapping typeMapping)
        {
            instance = ApplyDefaultTypeMapping(instance);
            var typeMappedArguments = new List<SqlExpression>();
            foreach (var argument in arguments)
            {
                typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
            }

            return new SqlFunctionExpression(
                instance,
                functionName,
                typeMappedArguments,
                returnType,
                typeMapping);
        }

        public SqlFunctionExpression SqlFunction(
            string functionName, bool niladic, Type returnType, RelationalTypeMapping typeMapping)
        {
            return new SqlFunctionExpression(functionName, niladic, returnType, typeMapping);
        }

        public SqlFunctionExpression SqlFunction(
            string schema, string functionName, bool niladic, Type returnType, RelationalTypeMapping typeMapping)
        {
            return new SqlFunctionExpression(schema, functionName, niladic, returnType, typeMapping);
        }

        public SqlFunctionExpression SqlFunction(
            SqlExpression instance, string functionName, bool niladic, Type returnType, RelationalTypeMapping typeMapping)
        {
            instance = ApplyDefaultTypeMapping(instance);
            return new SqlFunctionExpression(instance, functionName, niladic, returnType, typeMapping);
        }

        public SqlUnaryExpression Convert(SqlExpression operand, Type type, RelationalTypeMapping typeMapping)
        {
            return new SqlUnaryExpression(ExpressionType.Convert, operand, type, typeMapping);
        }
    }
}
