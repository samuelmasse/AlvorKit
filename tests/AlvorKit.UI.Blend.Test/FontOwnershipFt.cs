namespace AlvorKit;

/// <summary>Records native font lifetime calls while using the same FreeType backend as production Blend composition.</summary>
internal unsafe class FontOwnershipFt : FtBackend
{
    private int libraryOpens;
    private int libraryCloses;
    private int faceCloses;
    private int facesClosedBeforeLibrary;

    internal int LibraryOpens => libraryOpens;
    internal int LibraryCloses => libraryCloses;
    internal int FaceCloses => faceCloses;
    internal int FacesClosedBeforeLibrary => facesClosedBeforeLibrary;

    public override int InitFreeType(out nint library)
    {
        libraryOpens++;
        return base.InitFreeType(out library);
    }

    public override int DoneFace(FtFaceRec* face)
    {
        faceCloses++;
        return base.DoneFace(face);
    }

    public override int DoneFreeType(nint library)
    {
        libraryCloses++;
        facesClosedBeforeLibrary = faceCloses;
        return base.DoneFreeType(library);
    }
}
