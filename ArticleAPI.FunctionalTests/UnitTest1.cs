using System.Globalization;

namespace ArticleAPI.FunctionalTests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var time = DateTime.UtcNow;
        
        CultureInfo cultureInfo = new CultureInfo("en-US");
        
        time.ToString("MMMM dd, yyyy", cultureInfo);
    }
}