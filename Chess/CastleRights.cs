public class CastleRights
{
    public bool WKS { get; set; }
    public bool BKS { get; set; }
    public bool WQS { get; set; }
    public bool BQS { get; set; }

    public CastleRights(bool wks, bool bks, bool wqs, bool bqs)
    {
        WKS = wks;
        BKS = bks;
        WQS = wqs;
        BQS = bqs;
    }
}