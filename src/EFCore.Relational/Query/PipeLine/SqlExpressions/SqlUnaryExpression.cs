// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SqlUnaryExpression : SqlExpression
    {
        public SqlUnaryExpression(
            ExpressionType operatorType,
            SqlExpression operand,
            Type type,
            RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Check.NotNull(operand, nameof(operand));
            OperatorType = operatorType;
            Operand = operand;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var operand = (SqlExpression)visitor.Visit(Operand);

            return operand != Operand
                ? new SqlUnaryExpression(OperatorType, operand, Type, TypeMapping)
                : this;
        }

        public ExpressionType OperatorType { get; }
        public SqlExpression Operand { get; }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlUnaryExpression sqlUnaryExpression
                    && Equals(sqlUnaryExpression));

        private bool Equals(SqlUnaryExpression sqlUnaryExpression)
            => base.Equals(sqlUnaryExpression)
            && OperatorType == sqlUnaryExpression.OperatorType
            && Operand.Equals(sqlUnaryExpression.Operand);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ OperatorType.GetHashCode();
                hashCode = (hashCode * 397) ^ Operand.GetHashCode();

                return hashCode;
            }
        }
    }
}
