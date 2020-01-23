namespace Microsoft.TeamFoundation.GitApi
{
    public enum CheckoutIndexOptionStage
    {
        Default = 0, // implicitly means index for non-conflicts

        Stage1 = 1, // ancestor in merges
        Stage2 = 2, // frequently known as ours
        Stage3 = 3, // frequently known as theirs

        All = 4, // implies --temp
    }
}
