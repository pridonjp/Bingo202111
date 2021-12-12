using System;

namespace BingoWeb
{
    /// <summary>
    /// Azure Cosmos DBに保管するデータモデルは、このインターフェースを実装する
    /// </summary>
    public interface IBingo
    {
        string id { get; set; }
        string category { get; set; }
    }
}
