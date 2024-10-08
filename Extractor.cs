﻿using Discord;
using UnityEngine;

namespace ToolForExiled;

public class Extractor<TSource>
{

    private IEnumerable<TSource> sources;

    public Extractor<TSource> AddSource(IEnumerable<TSource> source)
    {
        if (sources == null)
            sources = source;
        else if (source != null)
            sources = sources.Concat(source);
        return this;
    }

    public Extractor<TSource> AddExtraction<TResult>(out QueryResult<TResult> result, TResult @default, bool remove = false)
    {
        result = new QueryResult<TResult>(out var setter);
        sources = GetExtraction(sources, setter, @default, remove);
        return this;
    }

    public void Execute()
    {
        var parse = sources.GetEnumerator();
        while (parse.MoveNext());
    }

    private IEnumerable<TSource> GetExtraction<TResult>(IEnumerable<TSource> source, Action<TResult> set, TResult @default, bool remove)
    {
        bool isSet = false;
        foreach (var item in source)
        {
            try
            {
                if (!isSet && item is TResult result)
                {
                    set(result);
                    isSet = true;
                    if (remove) continue;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error when extracting values {e}");
            }

            yield return item;
        }
        if (!isSet)
            set(@default);
    }
}

public class QueryResult<TResult>
{
    private TResult _value;
    private bool _define;

    public TResult Result
    {
        get
        {
            if (!_define) throw new InvalidOperationException("The result is not define.");
            return _value;
        }
    }

    public QueryResult(out Action<TResult> set)
    {
        set = (value) =>
        {
            _value = value;
            _define = true;
        };
    }

    public static implicit operator TResult(QueryResult<TResult> qr) => qr.Result;
}