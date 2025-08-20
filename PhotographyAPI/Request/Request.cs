using System.Collections.Concurrent;
using Amazon.S3.Model;

static class Request {
    static public bool Test(params string[] args)
    {
        foreach (string arg in args)
        {
            if (arg == null || string.IsNullOrEmpty(arg)) {
                return false;
            }
        }
        return true;
    }
}