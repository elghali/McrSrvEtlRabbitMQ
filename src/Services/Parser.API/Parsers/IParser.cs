namespace Parser.API.Parsers
{
    internal interface IParser
    {
        void ParseData(CancellationToken cancellationToken);
    }
}
