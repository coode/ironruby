﻿/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Dynamic;
using System.Linq.Expressions;

using Microsoft.Scripting.Actions;

using Ast = System.Linq.Expressions.Expression;
using Microsoft.Scripting.Runtime;
using System.Runtime.CompilerServices;

namespace IronPython.Runtime.Binding {
    interface IFastInvokable {
        FastBindResult<T> MakeInvokeBinding<T>(CallSite<T> site, PythonInvokeBinder/*!*/ binder, CodeContext/*!*/ state, object[] args) where T : class;
    }
}
