namespace TaskBoard.Models.Datatables;

public class DataTableAjaxModel
{
    // properties are not capital due to json mapping
    public int draw { get; set; }
    public int start { get; set; }
    public int length { get; set; }
    public List<Column> columns { get; set; }
    public Search search { get; set; }
    public List<Order> order { get; set; }
}
 
public class Column
{
    public string? data { get; set; }
    public string name { get; set; }
    public bool searchable { get; set; }
    public bool orderable { get; set; }
    public Search search { get; set; }
}
 
public class Search
{
    private string _value;
    public string value { 
        get => _value;
        set => _value = value.ToLowerInvariant();
    }
    
    public string regex { get; set; }
}
 
public class Order
{
    public int column { get; set; }
    public string dir { get; set; }
}

public class DataTablesResponse
{
    public int Draw { get; set; }
    public int RecordsTotal { get; set; }
    public int RecordsFiltered { get; set; }
    public object Data { get; set; }
}