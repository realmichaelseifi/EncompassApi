﻿using System.ComponentModel;
using System.Runtime.Serialization;

namespace EncompassApi.Loans.Enums
{
    /// <summary>
    /// LockRequestLoanPurposeType
    /// </summary>
    public enum LockRequestLoanPurposeType
    {
        /// <summary>
        /// Construction - Perm
        /// </summary>
        [Description("Construction - Perm")]
        ConstructionToPermanent = 0,
        /// <summary>
        /// No Cash-Out Refi
        /// </summary>
        [Description("No Cash-Out Refi")]
        [EnumMember(Value = "NoCash-Out Refinance")]
        NoCashOutRefinance = 1,
        /// <summary>
        /// Purchase
        /// </summary>
        Purchase = 2,
        /// <summary>
        /// Construction
        /// </summary>
        [Description("Construction")]
        ConstructionOnly = 3,
        /// <summary>
        /// Cash-Out Refi
        /// </summary>
        [Description("Cash-Out Refi")]
        [EnumMember(Value = "Cash-Out Refinance")]
        CashOutRefinance = 4,
        /// <summary>
        /// Other
        /// </summary>
        Other = 5,
        /// <summary>
        /// Construction
        /// </summary>
        Construction = 6
    }
}