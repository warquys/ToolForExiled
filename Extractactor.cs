using static ToolForExiled.Extractor;

namespace ToolForExiled;

public static class Extractor
{


    public static Source<TSoruce> AddSource<TSoruce>(IEnumerable<TSoruce> source)
    {

    }

    public static Source<TSoruce> AddRequest<TSoruce, TRequest>(out ExtractorResult<TRequest> request)
    {
        Source<TSoruce> t;
        t.
    }
    
    public static void Extract()
    {
    }

    public record Source<TSource>(IEnumerable<TSource> Sources);

    public record Request<TSource>(ExtractorResult<TRequest> result)
    {
        internal bool Parse(TSource source)
        {

        }
    }

}

public class ExtractorResult<TRequest>
{

    internal bool defined;
    internal TRequest value;

    public TRequest Value
    {
        get
        {
            if (defined == false)
                throw new InvalidOperationException("The value is not defined.");

            return value;
        }
    }

    public static implicit operator TRequest(ExtractorResult<TRequest> r) => r.value;
}