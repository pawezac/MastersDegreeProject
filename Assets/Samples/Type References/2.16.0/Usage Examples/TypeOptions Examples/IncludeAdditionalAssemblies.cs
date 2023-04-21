﻿namespace TypeReferences.Demo.TypeOptions_Examples
{
    using Utils;

    public class IncludeAdditionalAssemblies : TypeReferenceExample
    {
        [InfoBox("By default, only the types the class can reference directly are included in the drop-down " +
                 "list. In this example, CustomAssembly only has access to the System and TypeReferences assemblies.")]
        public NoAttributeStruct NoAttribute;

        [InfoBox("But when we use [TypeOptions(IncludeAdditionalAssemblies = new []{ \"Assembly-CSharp\" })], " +
                 "we get access to all the classes in Assembly-CSharp even though CustomAssembly doesn't have a " +
                 "reference to Assembly-CSharp.")]
        public DefaultAssemblyIncludedStruct DefaultAssemblyIncluded;
    }
}