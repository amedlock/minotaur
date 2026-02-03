using System;
using System.Collections.Generic;
using System.Linq;

namespace minotaur.Source;

/**
 * This RuleTable (or Decision Table) uses ints only
 *
 * Example use:
 * var table = RuleTable("a,b,c") // or load from file
 * table.AddRule("a = 100, b > 0")
 * table.AddRule("a = 200, b <= 5, c = 50.0")
 *
 * var env = table.Env();
 * env.set("a", 12)
 * env.set( "b", 100 )
 */
class Context
{
  private readonly RuleTable _ruleTable;
  private int[] _source;
  private int[] _dest;

  Context(RuleTable ruleTable)
  {
    _ruleTable = ruleTable;
    _source = new int[ruleTable.InCount];
    _dest = new int[ruleTable.OutCount];
  }

  public void Set(string var, int value)
  {
    _source[_ruleTable[var]] = value;
  }

  public int Get(string var)
  {
    return _source[_ruleTable[var]];
  }
}


public record Rule(string Name, Dictionary<string,int> Values);

public class Env(RuleTable ruleTable)
{
  private int[] inputs = new int[ruleTable.InCount];
  private int[] outputs = new int[ruleTable.OutCount];
}



public class RuleTable
{
  private String _name;
  private List<string> _inputVars;
  private List<string> _outputVars;
  private List<Rule> _rules = new();

  public RuleTable(string tableName, String inputVars, String outputVars)
  {
    _name = tableName;
    _inputVars = inputVars.Split(',').Select(s => s.Trim()).ToList();
    _outputVars = outputVars.Split(',').Select(s => s.Trim()).ToList();
  }

  public int InCount => _inputVars.Count;
  public int OutCount => _outputVars.Count;

  public void AddRule(string testRule, params (string var,int value)[] values)
  {
    var d = new  Dictionary<string, int>();
    foreach (var v in values)
    {
      d.Add(v.var, v.value);
    }
    _rules.Add(new Rule(testRule, d));
  }

  public int this[string varName] => _inputVars.IndexOf(varName);
}
