﻿using FishNet.Component.Prediction;
using Mono.Cecil;
using System;
using System.Reflection;

namespace FishNet.CodeGenerating.Helping
{
    internal class PredictedObjectHelper : CodegenBase
    {
        public override bool ImportReferences()
        {
            return true;
        }
    }
}