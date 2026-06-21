namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

/// <summary>Tests for importing Khronos DocBook reference pages into XML documentation.</summary>
[TestClass]
public sealed class GlDocParserTest
{
    /// <summary>Valid refpages map every synopsis function to escaped summary and parameter documentation.</summary>
    [TestMethod]
    public void Parse_ValidRefpage_MapsFunctionsAndParameters()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("glFixture.xml", """
            <!DOCTYPE refentry [
            <!ENTITY ignored "ignored">
            ]>
            <refentry xmlns="http://docbook.org/ns/docbook">
              <refnamediv>
                <refpurpose>Creates GL_TEXTUREi <parameter>objects</parameter> with glBindTexture &amp; buffers.</refpurpose>
              </refnamediv>
              <refsynopsisdiv>
                <funcsynopsis>
                  <funcprototype><funcdef>void <function>glCreateThings</function></funcdef></funcprototype>
                  <funcprototype><funcdef>void <function>glCreateThings</function></funcdef></funcprototype>
                  <funcprototype><funcdef>void <function>glCreateOtherThings</function></funcdef></funcprototype>
                </funcsynopsis>
              </refsynopsisdiv>
              <refsect1 xml:id="parameters">
                <title>Parameters</title>
                <variablelist>
                  <varlistentry>
                    <term><parameter>count</parameter></term>
                    <term><parameter>things</parameter></term>
                    <listitem>
                      <para>Number of <parameter>things</parameter><manvolnum>3</manvolnum> for GL_COLOR_ATTACHMENT$m$,
                      GL_NONE, and glDrawBuffer
                      <inlineequation xmlns:m="http://www.w3.org/1998/Math/MathML"><m:math /></inlineequation>
                      to create.</para>
                    </listitem>
                  </varlistentry>
                  <varlistentry>
                    <term><parameter>unused</parameter></term>
                    <listitem>   </listitem>
                  </varlistentry>
                </variablelist>
              </refsect1>
            </refentry>
            """);
        workspace.Write("glInvalid.xml", "<refentry>");
        workspace.Write("notGl.xml", "<refentry />");

        var docs = new GlDocParser().Parse(workspace.Root);

        Assert.AreEqual(2, docs.Count);
        Assert.AreEqual(
            "Creates <c>GL_TEXTUREi</c> objects with <c>glBindTexture</c> &amp; buffers.",
            docs["glCreateThings"].Summary);
        Assert.AreEqual(docs["glCreateThings"], docs["glCreateOtherThings"]);
        Assert.AreEqual(
            "Number of things for <c>GL_COLOR_ATTACHMENTi</c>, <c>GL_NONE</c>, and <c>glDrawBuffer</c> to create.",
            docs["glCreateThings"].Parameters["count"]);
        Assert.AreEqual(
            "Number of things for <c>GL_COLOR_ATTACHMENTi</c>, <c>GL_NONE</c>, and <c>glDrawBuffer</c> to create.",
            docs["glCreateThings"].Parameters["things"]);
        Assert.IsFalse(docs["glCreateThings"].Parameters.ContainsKey("unused"));
    }

    /// <summary>Refpages without synopsis functions or parameter sections are skipped or produce empty parameter docs.</summary>
    [TestMethod]
    public void Parse_MissingSections_SkipsEmptyFunctionsAndKeepsEmptyParameters()
    {
        using var workspace = TempWorkspace.Create();
        workspace.Write("glNoFunctions.xml", """
            <refentry xmlns="http://docbook.org/ns/docbook">
              <refsynopsisdiv><function>   </function></refsynopsisdiv>
              <refnamediv><refpurpose>ignored</refpurpose></refnamediv>
            </refentry>
            """);
        workspace.Write("glNoParameters.xml", """
            <refentry xmlns="http://docbook.org/ns/docbook">
              <refsynopsisdiv><function>glNoParameters</function></refsynopsisdiv>
              <refnamediv><refpurpose>   </refpurpose></refnamediv>
            </refentry>
            """);
        workspace.Write("glNoPurpose.xml", """
            <refentry xmlns="http://docbook.org/ns/docbook">
              <refsynopsisdiv><function>glNoPurpose</function></refsynopsisdiv>
            </refentry>
            """);

        var docs = new GlDocParser().Parse(workspace.Root);

        Assert.AreEqual(2, docs.Count);
        Assert.IsNull(docs["glNoParameters"].Summary);
        Assert.IsNull(docs["glNoPurpose"].Summary);
        Assert.AreEqual(0, docs["glNoParameters"].Parameters.Count);
    }
}
