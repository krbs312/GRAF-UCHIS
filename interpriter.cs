using System;
using System.Windows.Forms;
using System.Drawing;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using DiplomApp;


namespace DiplomApp
{
    enum Commands {
        moveto,
        move,
        setcolor,
        setdirection,
        rotate,
        mforward,
        mbackward,
        settext,
        interact,
        push,      
        remove,    
        _for_,
        _if_,
        _print_,
        paint

    }

    enum getcommands
    {
        getcolor,
        getdirection,
        getposition,
        gettext
    }

    enum LogOper
    {
        _and_,
        _or_,
        _not_
    }

    public enum VarType
    {
        _int_,
        _string_,
        _bool_,
        _object_,
        _array_      
    }

    public class Interpriter
    {



public Mapobject ObjectJoise(Mapobject obj, string script, int previousCommandEnd, int start, List<Variable> variables, List<Mapobject> mapObjects)
        {
                            Mapobject thisObj = obj;
                            int length = Math.Max(0, start - previousCommandEnd);
                            string line = length > 0 ? script.Substring(previousCommandEnd, length) : "";
                            line = line.Trim();

                            if (line.Contains("this.getnamef"))
                            {
                                Rotated rotated = obj as Rotated;
                                if (rotated != null)
                                {
                                    string forwardName = rotated.GetNameForward(mapObjects);
                                    thisObj = mapObjects.FirstOrDefault(o => o.name == forwardName);
                                    return thisObj;
                                }
                            }

                            if (line.Contains("this."))
                            {
                                thisObj = variables.FirstOrDefault(v => v.name == "this")?.value as Mapobject;
                            }
                            else
                            {
                                string nameobject = line.Trim().Split('.').FirstOrDefault() ?? "";
                                thisObj = mapObjects.FirstOrDefault(o => o.name == nameobject.Replace(';', ' ').Trim());
                            }
            return thisObj;
        }

public string EvaluateExpression(string expression)
{
    expression = expression ?? "";
    expression = expression.Trim();

    List<string> tokens = new List<string>();
    for (int i = 0; i < expression.Length; )
    {
        if (char.IsWhiteSpace(expression[i])) { i++; continue; }
        if (expression[i] == '(' || expression[i] == ')')
        {
            tokens.Add(expression[i].ToString());
            i++; continue;
        }
        if (i + 1 < expression.Length)
        {
            string twoChar = expression.Substring(i, 2);
            if (twoChar == ">="  || twoChar == "<="  || twoChar == "==" || twoChar == "!=")
            {
                tokens.Add(twoChar);
                i += 2;
                continue;
            }
        }
        if (expression[i] == '>' || expression[i] == '<' || expression[i] == '+' || expression[i] == '-' || expression[i] == '*' || expression[i] == '/')
        {
            tokens.Add(expression[i].ToString());
            i++; continue;
        }
        int j = i;
        while (j < expression.Length && (char.IsLetterOrDigit(expression[j]) || expression[j] == '_')) j++;
        if (j > i)
        {
            string w = expression.Substring(i, j - i).ToLower();
            tokens.Add(w);
            i = j;
            continue;
        }
        i++;
    }

    int pos = 0;
    bool ParseExpression(out object value)
    {
        return ParseOr(out value);
    }

    bool ParseOr(out object val)
    {
        if (!ParseAnd(out val)) return false;
        while (pos < tokens.Count && tokens[pos] == "or")
        {
            pos++;
            if (!ParseAnd(out object right)) { val = false; return false; }
            val = (bool)val || (bool)right;
        }
        return true;
    }

    bool ParseAnd(out object val)
    {
        if (!ParseComparison(out val)) return false;
        while (pos < tokens.Count && tokens[pos] == "and")
        {
            pos++;
            if (!ParseComparison(out object right)) { val = false; return false; }
            val = (bool)val && (bool)right;
        }
        return true;
    }

    bool ParseComparison(out object val)
    {
        if (!ParseAddSub(out val)) return false;
        if (pos < tokens.Count && IsComparisonOp(tokens[pos]))
        {
            string op = tokens[pos];
            pos++;
            if (!ParseAddSub(out object right)) { val = false; return false; }
            val = EvaluateComparison(val, op, right);
        }
        return true;
    }

    bool IsComparisonOp(string op)
    {
        return op == "==" || op == "!=" || op == "<" || op == ">" || op == "<=" || op == ">=";
    }

    object EvaluateComparison(object left, string op, object right)
    {
        if (left is bool && right is bool)
        {
            bool l = (bool)left;
            bool r = (bool)right;
            return op switch
            {
                "==" => l == r,
                "!=" => l != r,
                "<" => (l ? 1 : 0) < (r ? 1 : 0),
                ">" => (l ? 1 : 0) > (r ? 1 : 0),
                "<=" => (l ? 1 : 0) <= (r ? 1 : 0),
                ">=" => (l ? 1 : 0) >= (r ? 1 : 0),
                _ => false
            };
        }
        else
        {
            double l = Convert.ToDouble(left);
            double r = Convert.ToDouble(right);
            return op switch
            {
                "==" => l == r,
                "!=" => l != r,
                "<" => l < r,
                ">" => l > r,
                "<=" => l <= r,
                ">=" => l >= r,
                _ => false
            };
        }
    }

    bool ParseAddSub(out object val)
    {
        if (!ParseMulDiv(out val)) return false;
        while (pos < tokens.Count && (tokens[pos] == "+" || tokens[pos] == "-"))
        {
            string op = tokens[pos];
            pos++;
            if (!ParseMulDiv(out object right)) return false;
            double leftNum = Convert.ToDouble(val);
            double rightNum = Convert.ToDouble(right);
            val = op == "+" ? leftNum + rightNum : leftNum - rightNum;
        }
        return true;
    }

    bool ParseMulDiv(out object val)
    {
        if (!ParseNot(out val)) return false;
        while (pos < tokens.Count && (tokens[pos] == "*" || tokens[pos] == "/"))
        {
            string op = tokens[pos];
            pos++;
            if (!ParseNot(out object right)) return false;
            double leftNum = Convert.ToDouble(val);
            double rightNum = Convert.ToDouble(right);
            val = op == "*" ? leftNum * rightNum : leftNum / rightNum;
        }
        return true;
    }

    bool ParseNot(out object val)
    {
        if (pos < tokens.Count && tokens[pos] == "not")
        {
            pos++;
            if (!ParseNot(out object inner)) { val = false; return false; }
            val = !(bool)inner;
            return true;
        }
        return ParsePrimary(out val);
    }

    bool ParsePrimary(out object val)
    {
        if (pos >= tokens.Count) { val = false; return false; }
        string t = tokens[pos];
        if (t == "(")
        {
            pos++;
            if (!ParseExpression(out object inner)) { val = false; return false; }
            if (pos < tokens.Count && tokens[pos] == ")") pos++;
            val = inner;
            return true;
        }
        if (t == "true") { pos++; val = true; return true; }
        if (t == "false") { pos++; val = false; return true; }
        if (double.TryParse(t, out double num)) { pos++; val = num; return true; }
        pos++; val = false; return true;
    }

    if (tokens.Count == 0) return "false";

    pos = 0;
    if (ParseExpression(out object result))
    {
        return result is bool b ? b.ToString().ToLower() : result.ToString();
    }
    MessageBox.Show("Ошибка разбора выражения");
    return "false";
}

 



        public bool _and(bool a, bool b)
        {
            return a && b;
        }

        public bool _or(bool a, bool b)
        {
            return a || b;
        }
        public bool _not(bool a)
        {
            return !a;
        }
        public bool _equal(string a, string b)
        {
            return a == b;
        }
        public class Variable
        {
            public string name;
            public VarType type;
            public object? value;

            public Variable(string name, VarType type, object? value)
            {
                this.name = name;
                this.type = type;
                this.value = value;
            }
        }

        public class FunctionDef
        {
            public string name;
            public List<string> parameters;
            public string body;

            public FunctionDef(string name, List<string> parameters, string body)
            {
                this.name = name;
                this.parameters = parameters;
                this.body = body;
            }
        }

        private List<string> SplitTopLevelCommas(string s)
        {
            List<string> parts = new List<string>();
            int depth = 0;
            var sb = new System.Text.StringBuilder();
            foreach (char c in s)
            {
                if (c == '(') depth++;
                else if (c == ')') depth--;
                if (c == ',' && depth == 0)
                {
                    parts.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            if (sb.Length > 0) parts.Add(sb.ToString().Trim());
            return parts;
        }

        private string ExtractParameters(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            int start = text.IndexOf('(');
            if (start == -1) return "";
            int depth = 0;
            for (int i = start; i < text.Length; i++)
            {
                if (text[i] == '(')
                {
                    depth++;
                }
                else if (text[i] == ')')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return text.Substring(start + 1, i - start - 1);
                    }
                }
            }
            return "";
        }

        public object? unpackvar(string varname, List<Variable> variables, List<FunctionDef> functions, Mapobject obj, List<Mapobject> mapObjects, List<(Color, int,int)> mapcolors)
        {
            varname = varname?.Trim() ?? "";
            int paren = varname.IndexOf('(');
            if (paren != -1 && varname.EndsWith(")"))
            {
                string fname = varname.Substring(0, paren).Trim();
                string inside = ExtractParameters(varname);
                var args = SplitTopLevelCommas(inside).Where(p => p.Length > 0).ToList();
                FunctionDef? func = functions.FirstOrDefault(f => f.name == fname);
                if (func != null)
                {
                    return CallFunction(func, args, variables, functions, obj, mapObjects, mapcolors);
                }
                else
                {
                }
            }

            if (varname.Contains("[") && varname.Contains("]"))
            {
                int open = varname.IndexOf('[');
                int close = varname.IndexOf(']');
                string baseName = varname.Substring(0, open);
                string idxStr = varname.Substring(open + 1, close - open - 1);
                if (int.TryParse(idxStr, out int idx))
                {
                    Variable? parent = variables.FirstOrDefault(v => v.name == baseName);
                    if (parent != null && parent.type == VarType._array_ && parent.value is System.Collections.IList list)
                    {
                        if (idx >= 0 && idx < list.Count)
                            return list[idx];
                    }
                }
                return null;
            }

            Variable? variable = variables.FirstOrDefault(v => v.name == varname);
            if (variable != null)
            {
                return variable.value;
            }
            return null;
        }

        private object? CallFunction(FunctionDef func, List<string> rawArgs, List<Variable> variables, List<FunctionDef> functions, Mapobject obj, List<Mapobject> mapObjects, List<(Color, int,int)> mapcolors)
        {
            List<Variable> savedVars = new List<Variable>(variables);
            int origCount = variables.Count;
            for (int i = 0; i < rawArgs.Count; i++)
            {
                string a = rawArgs[i];
                object? val = unpackvar(a, variables, functions, obj, mapObjects, mapcolors) ?? (object)a.Trim('"');
                VarType vt;
                if (val is int) vt = VarType._int_;
                else if (val is bool) vt = VarType._bool_;
                else if (val is string) vt = VarType._string_;
                else if (val is System.Collections.IList) vt = VarType._array_;
                else vt = VarType._object_;
                variables.Add(new Variable(func.parameters[i], vt, val));
            }
            string prevScript = obj.script;
            obj.script = func.body;
            object? ret = ExecuteScript(obj, mapObjects, mapcolors, variables, functions);
            obj.script = prevScript;
            while (variables.Count > origCount) variables.RemoveAt(variables.Count - 1);
            return ret;
        }

        private string ReplaceGetCommands(string text, Mapobject obj, List<Mapobject> mapObjects)
        {
            if (obj == null) return text;
            
            text = text.Replace("this.getcolor()", obj.color.Name);
            text = text.Replace("this.getcolor", obj.color.Name);

            Rotated rotated = obj as Rotated;
            if (rotated != null)
            {
                text = text.Replace("this.getdirection()", $"\"{rotated.direction}\"");
                text = text.Replace("this.getdirection", $"\"{rotated.direction}\"");

                text = text.Replace("this.getnamef()", $"\"{rotated.GetNameForward(mapObjects)}\"");
                text = text.Replace("this.getnamef", $"\"{rotated.GetNameForward(mapObjects)}\"");
            }

            string positionValue = $"\"{obj.position.X},{obj.position.Y}\"";
            text = text.Replace("this.getposition()", positionValue);
            text = text.Replace("this.getposition", positionValue);

            CubeText cubeText = obj as CubeText;
            if (cubeText != null)
            {
                text = text.Replace("this.gettext()", $"\"{cubeText.text}\"");
                text = text.Replace("this.gettext", $"\"{cubeText.text}\"");
            }

            return text;
        }


        public object? ExecuteScript(Mapobject obj, List<Mapobject> mapObjects, List<(Color, int,int)> mapcolors, List<Variable>? variables = null, List<FunctionDef>? functions = null)
        {
            string checkScript = obj?.script ?? "";
            int parenDepth = 0;
            foreach (char c in checkScript)
            {
                if (c == '(') parenDepth++;
                else if (c == ')') parenDepth--;
            }
            if (parenDepth != 0)
            {
                MessageBox.Show("Syntax error: unbalanced parentheses in script.\n" + checkScript);
                return null;
            }

            object? returnValue = null;
            bool hasReturn = false;

                if (variables == null)
                {
                    variables = new List<Variable>();
                    variables.Add(new Variable("this", VarType._object_, obj));
                }
                else
                {
                    Variable? self = variables.FirstOrDefault(v => v.name == "this");
                    if (self != null)
                        self.value = obj;
                    else
                        variables.Add(new Variable("this", VarType._object_, obj));
                }

                string script = obj.script;

                List<FunctionDef> localFunctions;
                if (functions == null)
                {
                    localFunctions = new List<FunctionDef>();
                    string working = script;
                    int searchIndex = 0;
                    while (true)
                    {
                        int idxFunc = working.IndexOf("function", searchIndex, StringComparison.Ordinal);
                        int idxProc = working.IndexOf("procedure", searchIndex, StringComparison.Ordinal);
                        int idx;
                        string keyword;
                        if (idxFunc != -1 && (idxProc == -1 || idxFunc < idxProc))
                        {
                            idx = idxFunc;
                            keyword = "function";
                        }
                        else if (idxProc != -1)
                        {
                            idx = idxProc;
                            keyword = "procedure";
                        }
                        else
                        {
                            break;
                        }

                        if (idx > 0 && char.IsLetterOrDigit(working[idx - 1]))
                        {
                            searchIndex = idx + keyword.Length;
                            continue;
                        }

                        int nameStart = idx + keyword.Length;
                        while (nameStart < working.Length && char.IsWhiteSpace(working[nameStart])) nameStart++;
                        int nameEnd = nameStart;
                        while (nameEnd < working.Length && (char.IsLetterOrDigit(working[nameEnd]) || working[nameEnd] == '_')) nameEnd++;
                        string fname = working.Substring(nameStart, nameEnd - nameStart);

                        int parOpen = working.IndexOf('(', nameEnd);
                        if (parOpen == -1) break;
                        int parClose = working.IndexOf(')', parOpen);
                        if (parClose == -1) break;
                        string paramList = working.Substring(parOpen + 1, parClose - parOpen - 1);
                        List<string> paramNames = paramList.Split(',').Select(p => p.Trim()).Where(p => p.Length > 0).ToList();

                        int braceOpen = working.IndexOf('{', parClose);
                        if (braceOpen == -1) break;
                        int braceCount = 0;
                        int braceClose = -1;
                        for (int i = braceOpen; i < working.Length; i++)
                        {
                            if (working[i] == '{') braceCount++;
                            else if (working[i] == '}')
                            {
                                braceCount--;
                                if (braceCount == 0)
                                {
                                    braceClose = i;
                                    break;
                                }
                            }
                        }
                        if (braceClose == -1) break;
                        string body = working.Substring(braceOpen + 1, braceClose - braceOpen - 1);

                        localFunctions.Add(new FunctionDef(fname, paramNames, body));

                        working = working.Remove(idx, braceClose - idx + 1);
                        searchIndex = idx;
                    }
                    script = working;
                }
                else
                {
                    localFunctions = functions;
                }

                string[] baseTypes = new[] { "object", "int", "string", "bool" };
                foreach (string variable in baseTypes)
                {
                    foreach (bool isArray in new[] { false, true })
                    {
                        string keyword = variable + (isArray ? "[]" : "") + " ";
                        int index = 0;
                        while ((index = script.IndexOf(keyword, index)) != -1)
                        {
                            int endIndex = script.IndexOf(';', index);
                            if (endIndex == -1) break; 
                            string decl = script.Substring(index, endIndex - index);
                            string[] parts = decl.Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 3 && parts[0] == variable + (isArray ? "[]" : ""))
                            {
                                string varName = parts[1];
                                string varValueStr = parts[2];
                                object? varValue = null;
                                if (!isArray)
                                {
                                    object? resolved = unpackvar(varValueStr, variables, localFunctions, obj, mapObjects, mapcolors);
                                    if (resolved != null)
                                    {
                                        varValue = resolved;
                                    }
                                }
                                if (isArray)
                                {
                                    string inner = varValueStr.Trim();
                                    inner = inner.Trim('{', '}');
                                    var items = inner.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(s => s.Trim())
                                                     .ToList();
                                    var list = new List<object>();
                                    foreach (var item in items)
                                    {
                                        object converted = variable switch
                                        {
                                            "object" => (object)item.Trim('"'),
                                            "int" => int.TryParse(item, out int iv) ? iv : 0,
                                            "string" => (object)item.Trim('"'),
                                            "bool" => bool.TryParse(item, out bool bv) ? bv : false,
                                            _ => item
                                        };
                                        list.Add(converted);
                                    }
                                    varValue = list;
                                }
                                else
                                {
                                    if (varValue == null)
                                    {
                                        varValue = variable switch
                                        {
                                            "object" => varValueStr.Trim('"'),
                                            "int" => int.TryParse(varValueStr, out int iv) ? iv : 0,
                                            "string" => varValueStr.Trim('"'),
                                            "bool" => bool.TryParse(varValueStr, out bool bv) ? bv : false,
                                            _ => null
                                        };
                                    }
                                }
                                VarType vt = isArray ? VarType._array_ : (VarType)Enum.Parse(typeof(VarType), "_" + variable + "_");
                                variables.Add(new Variable(varName, vt, varValue));
                            }
                            index = endIndex + 1;
                        }
                    }
                }



                List<(int start, int end)> commandPositions = new List<(int start, int end)>();
                foreach (string command in Enum.GetNames(typeof(Commands)).Select(c => c.Replace("_", "")))
                {
                    int index = 0;
                    while ((index = script.IndexOf(command, index)) != -1)
                    {
                        commandPositions.Add((index, index + command.Length));
                        index += command.Length;
                    }
                }
                {
                    int index = 0;
                    string keyword = "return";
                    while ((index = script.IndexOf(keyword, index)) != -1)
                    {
                        bool leftOk = index == 0 || !char.IsLetterOrDigit(script[index - 1]);
                        bool rightOk = index + keyword.Length >= script.Length || !char.IsLetterOrDigit(script[index + keyword.Length]);
                        if (leftOk && rightOk)
                        {
                            commandPositions.Add((index, index + keyword.Length));
                        }
                        index += keyword.Length;
                    }
                }
                foreach (var func in localFunctions)
                {
                    int index = 0;
                    string callPattern = func.name + "(";
                    while ((index = script.IndexOf(callPattern, index, StringComparison.Ordinal)) != -1)
                    {
                        commandPositions.Add((index, index + func.name.Length));
                        index += callPattern.Length;
                    }
                }
                commandPositions = commandPositions.OrderBy(p => p.start).ToList();
                List<(int start, int end)> skipcommands = new List<(int start, int end)>();
                int previousCommandEnd = 0;
                string previousCommand = "";
                foreach ((int start, int end) in commandPositions)
                {
                    
                    if (skipcommands.Any(c => start >= c.start && end <= c.end))
                    {
                        previousCommand = script.Substring(start, end - start);
                        continue;
                    }
                    
                    string command = script.Substring(start, end - start);
                    int paramPos = script.IndexOf('(', end);
                    string parameters = "";
                    if (paramPos != -1)
                    {
                        parameters = ExtractParameters(script.Substring(paramPos));
                    }

                    Mapobject thisObj = ObjectJoise(obj, script, previousCommandEnd, start, variables, mapObjects);
                    
                    if (thisObj == null)
                    {
                        thisObj = obj;
                    }
                    
                    parameters = ReplaceGetCommands(parameters, thisObj, mapObjects);
                    Map_info map_Info = new Map_info();
                    switch (command)
                    {
                        case "return":
                            {
                                string expr = parameters.Trim();
                                object? val = unpackvar(expr, variables, localFunctions, obj, mapObjects, mapcolors);
                                if (val == null)
                                {
                                    foreach (var v in variables)
                                    {
                                        if (v.value != null)
                                        {
                                            expr = Regex.Replace(expr, $@"\b{Regex.Escape(v.name)}\b", v.value.ToString()!);
                                        }
                                    }
                                    try
                                    {
                                        var result = new System.Data.DataTable().Compute(expr, null);
                                        val = result;
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("Compute exception: " + ex.Message);
                                        val = expr;
                                    }
                                }
                                returnValue = val;
                                hasReturn = true;
                            }
                            break;
                        case "push":
                            {
                                var parts = parameters.Split(',').Select(p => p.Trim()).ToList();
                                if (parts.Count >= 2)
                                {
                                    string name = parts[0];
                                    string valStr = parts[1];
                                    object? value = unpackvar(valStr, variables, localFunctions, obj, mapObjects, mapcolors) ?? (object)valStr.Trim('"');
                                    var variable = variables.FirstOrDefault(v => v.name == name);
                                    if (variable != null && variable.type == VarType._array_ && variable.value is System.Collections.IList list)
                                    {
                                        list.Add(value);
                                    }
                                }
                            }
                            break;
                        case "remove":
                            {
                                var parts = parameters.Split(',').Select(p => p.Trim()).ToList();
                                if (parts.Count >= 2 && int.TryParse(parts[1], out int idx))
                                {
                                    string name = parts[0];
                                    var variable = variables.FirstOrDefault(v => v.name == name);
                                    if (variable != null && variable.type == VarType._array_ && variable.value is System.Collections.IList list)
                                    {
                                        if (idx >= 0 && idx < list.Count)
                                            list.RemoveAt(idx);
                                    }
                                }
                            }
                            break;
                        case "paint":
                            foreach (string p in parameters.Split(',').Select(p => p.Trim()))
                            {
                                object? value = unpackvar(p, variables, localFunctions, obj, mapObjects, mapcolors);
                                if (value != null)
                                {
                                    parameters = parameters.Replace(p, value.ToString()!);
                                }
                            }
                            if (thisObj == null)
                            {
                                MessageBox.Show("Ошибка: не найден объект для команды paint");
                                continue;
                            }
                            if (!thisObj.canscriptable) return null;
                            
                            var paintParts = parameters.Split(',').Select(p => p.Trim()).ToList();
                            if (paintParts.Count == 3 && 
                                int.TryParse(paintParts[0], out int pr) && 
                                int.TryParse(paintParts[1], out int pg) && 
                                int.TryParse(paintParts[2], out int pb))
                            {
                                Color Colorp = Color.FromArgb(pr, pg, pb);
                                mapcolors.Add((Colorp, (int)thisObj.position.X, (int)thisObj.position.Y));
                            }
                            else
                            {
                                MessageBox.Show("Ошибка: paint требует формат (R, G, B) где R, G, B - числа от 0 до 255");
                            }
                            break;
                        case "interact":
                            foreach (string p in parameters.Split(',').Select(p => p.Trim()))
                            {
                                object? value = unpackvar(p, variables, localFunctions, obj, mapObjects, mapcolors);
                                if (value != null)
                                {
                                    parameters = parameters.Replace(p, value.ToString()!);
                                }
                            }

                            if (thisObj == null)
                            {
                                MessageBox.Show("Ошибка: не найден объект для команды interact");
                                continue;
                            }

                            Buttongi bt = null;
                            if (thisObj is Buttongi b1)
                            {
                                bt = b1;
                            }
                            else if (thisObj is Player player)
                            {
                                bt = map_Info.Getbyname(player.GetNameForward(mapObjects), mapObjects) as Buttongi;
                            }

                            string paramName = parameters.Trim().Trim('"');
                            if (!string.IsNullOrEmpty(paramName))
                            {
                                Buttongi byName = map_Info.Getbyname(paramName, mapObjects) as Buttongi;
                                if (byName != null)
                                    bt = byName;
                            }

                            if (bt != null)
                            {
                                bt.activate(mapObjects);
                            }
                            else
                            {
                                MessageBox.Show("Кнопка для команды interact не найдена");
                            }
                            break;
                        case "settext":
                            foreach (string p in parameters.Split(',').Select(p => p.Trim()))
                            {
                                object? value = unpackvar(p, variables, localFunctions, obj, mapObjects, mapcolors);
                                if (value != null)
                                {
                                    parameters = parameters.Replace(p, value.ToString()!);
                                }
                            }

                            if (thisObj == null)
                            {
                                MessageBox.Show("Ошибка: не найден объект для команды settext");
                                continue;
                            }
                            if (!thisObj.canscriptable) return null;
                            CubeText cubeTextObj = thisObj as CubeText;
                            if (cubeTextObj != null)
                            {
                                cubeTextObj.SetText(parameters.Trim().Trim('"'));
                            }
                            break;
                        case "setcolor":
                            foreach (string p in parameters.Split(',').Select(p => p.Trim()))
                            {
                                object? value = unpackvar(p, variables, localFunctions, obj, mapObjects, mapcolors);
                                if (value != null)
                                {
                                    parameters = parameters.Replace(p, value.ToString()!);
                                }
                            }

                            if (thisObj == null)
                            {
                                MessageBox.Show("Ошибка: не найден объект для команды setcolor");
                                continue;
                            }
                            if (!thisObj.canscriptable) return null;
                            
                            var rgbParts = parameters.Split(',').Select(p => p.Trim()).ToList();
                            if (rgbParts.Count == 3 && 
                                int.TryParse(rgbParts[0], out int r) && 
                                int.TryParse(rgbParts[1], out int g) && 
                                int.TryParse(rgbParts[2], out int b))
                            {
                                Color newColor = Color.FromArgb(r, g, b);
                                thisObj.SetColor(newColor);
                            }
                            else
                            {
                                MessageBox.Show("Ошибка: setcolor требует формат (R, G, B) где R, G, B - числа от 0 до 255");
                            }
                            break;
                        case "setdirection":
                            foreach (string p in parameters.Split(',').Select(p => p.Trim()))
                            {
                                object? value = unpackvar(p, variables, localFunctions, obj, mapObjects, mapcolors);
                                if (value != null)
                                {
                                    parameters = parameters.Replace(p, value.ToString()!);
                                }
                            }

                            if (thisObj == null)
                            {
                                MessageBox.Show("Ошибка: не найден объект для команды setdirection");
                                continue;
                            }
                            if (!thisObj.canscriptable) return null;
                            string direction = parameters.Trim().Trim('"');
                            Rotated rotatedObj = thisObj as Rotated;
                            if (rotatedObj != null)
                            {
                                rotatedObj.ChangeDirection(direction);
                            }
                            break;
                        case "rotate":
                            foreach (string p in parameters.Split(',').Select(p => p.Trim()))
                            {
                                object? value = unpackvar(p, variables, localFunctions, obj, mapObjects, mapcolors);
                                if (value != null)
                                {
                                    parameters = parameters.Replace(p, value.ToString()!);
                                }
                            }
                            if (thisObj == null)
                            {
                                MessageBox.Show("Ошибка: не найден объект для команды rotate");
                                continue;
                            }
                            if (!thisObj.canscriptable) return null;
                            Rotated rotObj = thisObj as Rotated;
                            if (rotObj != null)
                            {
                                rotObj.Rotate(parameters.Trim().Trim('"'));
                            }
                            break;
                        case "mforward":
                            foreach (string p in parameters.Split(',').Select(p => p.Trim()))
                            {
                                object? value = unpackvar(p, variables, localFunctions, obj, mapObjects, mapcolors);
                                if (value != null)
                                {
                                    parameters = parameters.Replace(p, value.ToString()!);
                                }
                            }
                            if (thisObj == null)
                            {
                                MessageBox.Show("Ошибка: не найден объект для команды mforvard");
                                continue;
                            }
                            if (!thisObj.canscriptable) return null;
                            Rotated rotatedObj1 = thisObj as Rotated;
                            if (rotatedObj1 != null){
                            rotatedObj1.MoveForward(mapObjects);
                            }
                            break;
                        case "mbackward":
                            foreach (string p in parameters.Split(',').Select(p => p.Trim()))
                            {
                                object? value = unpackvar(p, variables, localFunctions, obj, mapObjects, mapcolors);
                                if (value != null)
                                {
                                    parameters = parameters.Replace(p, value.ToString()!);
                                }
                            }
                            if (thisObj == null)
                            {
                                MessageBox.Show("Ошибка: не найден объект для команды mbackward");
                                continue;
                            }
                            if (!thisObj.canscriptable) return null;
                            Rotated rotatedObj2 = thisObj as Rotated;
                            if (rotatedObj2 != null){
                                rotatedObj2.MoveBackward(mapObjects);
                            }
                            break;
                        case "moveto":
                            foreach (string p in parameters.Split(',').Select(p => p.Trim()))
                            {
                                object? value = unpackvar(p, variables, localFunctions, obj, mapObjects, mapcolors);
                                if (value != null)
                                {
                                    parameters = parameters.Replace(p, value.ToString()!);
                                }
                            }

                            string s = parameters.Trim();
                            if (thisObj == null)
                            {
                                MessageBox.Show("Ошибка: не найден объект для команды moveto");
                                continue;
                            }
                            if (!thisObj.canscriptable) return null;
                            if (s == "up" || s == "down" || s == "left" || s == "right")
                            {
                                thisObj.MoveByOneCell(s, mapObjects);
                            }
                            break;
                        case "move":
                            List<string> par = parameters.Split(',').Select(p => p.Trim()).ToList();
                            foreach (string p in par){
                                object? value = unpackvar(p, variables, localFunctions, obj, mapObjects, mapcolors);
                                if (value != null)
                                {
                                    parameters = parameters.Replace(p, value.ToString()!);
                                }
                            }
                            par = parameters.Split(',').Select(p => p.Trim()).ToList();


                            if (thisObj == null)
                            {
                                MessageBox.Show("Ошибка: не найден объект для команды move");
                                continue;
                            }
                            if (!thisObj.canscriptable) return null;
                            if (par.Count == 2 && int.TryParse(par[0], out int x) && int.TryParse(par[1], out int y))
                            {
                                thisObj.Move(new Vector3(x, -y, 0), mapObjects);
                            }
                            break;
                        case "for":
                            List<string> parfor = parameters.Split(';').Select(p => p.Trim()).ToList();
                            if (parfor.Count < 3)
                            {
                                MessageBox.Show($"Invalid for loop parameters: {parameters}");
                                continue;
                            }
                            List<string> var = parfor[0].Split('=').Select(p => p.Trim()).ToList();
                            if (var.Count < 2)
                            {
                                MessageBox.Show($"Invalid for loop variable declaration: {parfor[0]}");
                                continue;
                            }
                            foreach (string p in var)
                            {
                                object? value = unpackvar(p, variables, localFunctions, obj, mapObjects, mapcolors);
                                if (value != null)
                                {
                                    parameters = parameters.Replace(p, value.ToString()!);
                                }
                            }
                            string namevar = var[0];
                            if (!int.TryParse(var[1], out int startfor))
                            {
                                MessageBox.Show($"Invalid for loop start value: {var[1]}");
                                continue;
                            }
                            string conditionStr = parfor[1];
                            string operatorfor = conditionStr.Contains("<=") ? "<=" : conditionStr.Contains(">=") ? ">=" : conditionStr.Contains("<") ? "<" : conditionStr.Contains(">") ? ">" : "";
                            if (operatorfor == "")
                            {
                                MessageBox.Show($"Invalid for loop condition: {conditionStr}");
                                continue;
                            }
                            string[] condition = conditionStr.Split(new string[] { "<=", ">=", "<", ">" }, StringSplitOptions.None).Select(p => p.Trim()).ToArray();
                            if (condition.Length < 2 || !int.TryParse(condition[1], out int endfor))
                            {
                                MessageBox.Show($"Invalid for loop end value: {conditionStr}");
                                continue;
                            }
                            string changePart = parfor[2];
                            string operatorchange = changePart.Contains("++") ? "++" : changePart.Contains("--") ? "--" : "";
                            if (operatorchange == "")
                            {
                                MessageBox.Show($"Invalid for loop change operator: {changePart}");
                                continue;
                            }
                            string changevar = changePart.Split(new string[] { "++", "--" }, StringSplitOptions.None).Select(p => p.Trim()).ToList()[0];


                            List<int> allbraces = new List<int>();
                            List<int> allbraces2 = new List<int>();
                            int indexbrace = 0;
                            while ((indexbrace = script.IndexOf('{', indexbrace)) != -1)
                            {
                                allbraces.Add(indexbrace);
                                indexbrace++;
                            }
                            indexbrace = 0;
                            while ((indexbrace = script.IndexOf('}', indexbrace)) != -1)
                            {
                                allbraces2.Add(indexbrace);
                                indexbrace++;
                            }
                            List<(int start, int end)> braces = new List<(int start, int end)>();
                            foreach (int startbrace in allbraces)
                            {
                                int endbrace = allbraces2.FirstOrDefault(b => b > startbrace);
                                if (endbrace != 0)
                                {
                                    braces.Add((startbrace, endbrace));
                                    allbraces2.Remove(endbrace);
                                }
                            }
                            int deletestrat = 0;
                            int deleteend = 0;
                            string body = "";
                            foreach ((int startbrace, int endbrace) in braces)
                            {
                                if (startbrace > end)
                                {
                                    if (endbrace > startbrace + 1)
                                    {
                                        body = script.Substring(startbrace + 1, endbrace - startbrace - 1);
                                        deletestrat = startbrace;
                                        deleteend = endbrace;
                                    }
                                    break;
                                }
                            }
                            if (string.IsNullOrEmpty(body))
                            {
                                MessageBox.Show($"For loop body not found for for(...) starting at position {end}");
                                continue;
                            }

                            string oldscript = obj.script;
                            if (operatorchange == "++")
                            {
                                
                                for (int i = startfor; operatorfor == "<=" ? i <= endfor : operatorfor == ">=" ? i >= endfor : operatorfor == "<" ? i < endfor : i > endfor; i++)
                                {
                                    string newscript = Regex.Replace(body, $"\\b{Regex.Escape(namevar)}\\b", i.ToString());
                                    obj.script = newscript;
                                    this.ExecuteScript(obj, mapObjects, mapcolors, variables, localFunctions);
                                }
                            }
                            else if (operatorchange == "--")
                            {
                                for (int i = startfor; operatorfor == "<=" ? i <= endfor : operatorfor == ">=" ? i >= endfor : operatorfor == "<" ? i < endfor : i > endfor; i--)
                                {
                                    string newscript = Regex.Replace(body, $"\\b{Regex.Escape(namevar)}\\b", i.ToString());
                                    obj.script = newscript;
                                    this.ExecuteScript(obj, mapObjects, mapcolors, variables, localFunctions);
                                }
                            }
                            obj.script = oldscript;
                            skipcommands.Add((deletestrat, deleteend));
                                                     

                            break;
                        case "if":

                            int ifOpenParen = script.IndexOf('(', end);
                            int ifCloseParen = script.IndexOf(')', ifOpenParen);
                            
                            if (ifOpenParen != -1 && ifCloseParen != -1)
                            {
                                string condition1 = script.Substring(ifOpenParen + 1, ifCloseParen - ifOpenParen - 1);
                                foreach (string p in condition1.Split(new char[] { ' ', '(', ')', '>', '<', '=', '!' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
                                {
                                    object? value = unpackvar(p, variables, localFunctions, obj, mapObjects, mapcolors);
                                    if (value != null)
                                    {
                                        condition1 = condition1.Replace(p, value.ToString()!);
                                    }
                                }
                                int bodyStart = script.IndexOf('{', ifCloseParen);
                                int bodyEnd = script.IndexOf('}', bodyStart);
                                
                                int elseBodyStart = -1;
                                int elseBodyEnd = -1;
                                
                                if (bodyStart != -1 && bodyEnd != -1)
                                {
                                    skipcommands.Add((bodyStart, bodyEnd));
                                    
                                    // Проверяем наличие else
                                    int elsePos = bodyEnd + 1;
                                    while (elsePos < script.Length && char.IsWhiteSpace(script[elsePos])) elsePos++;
                                    
                                    if (elsePos < script.Length - 4 && script.Substring(elsePos, 4) == "else")
                                    {
                                        elsePos += 4;
                                        while (elsePos < script.Length && char.IsWhiteSpace(script[elsePos])) elsePos++;
                                        
                                        if (elsePos < script.Length && script[elsePos] == '{')
                                        {
                                            elseBodyStart = elsePos;
                                            elseBodyEnd = script.IndexOf('}', elseBodyStart);
                                            
                                            if (elseBodyStart != -1 && elseBodyEnd != -1)
                                            {
                                                skipcommands.Add((elseBodyStart, elseBodyEnd));
                                            }
                                        }
                                    }
                                }

                                string conditionResult = EvaluateExpression(condition1);

                                if (conditionResult == "true")
                                {
                                    if (bodyStart != -1 && bodyEnd != -1)
                                    {
                                        string bodyif = script.Substring(bodyStart + 1, bodyEnd - bodyStart - 1);
                                        string prev = obj.script;
                                        obj.script = bodyif;
                                        this.ExecuteScript(obj, mapObjects, mapcolors, variables, localFunctions);
                                        obj.script = prev;
                                    }
                                }
                                else
                                {
                                    if (elseBodyStart != -1 && elseBodyEnd != -1)
                                    {
                                        string bodyelse = script.Substring(elseBodyStart + 1, elseBodyEnd - elseBodyStart - 1);
                                        string prev = obj.script;
                                        obj.script = bodyelse;
                                        this.ExecuteScript(obj, mapObjects, mapcolors, variables, localFunctions);
                                        obj.script = prev;
                                    }
                                }
                            }
                            break;
                        case "print":
                            {
                                string expr = parameters.Trim();
                                object? val = unpackvar(expr, variables, localFunctions, obj, mapObjects, mapcolors);
                                if (val != null)
                                {
                                    parameters = val.ToString();
                                }
                                else
                                {
                                    string evalResult = EvaluateExpression(expr);
                                    if (evalResult != "false" || expr.Contains("+") || expr.Contains("-") || expr.Contains("*") || expr.Contains("/"))
                                    {
                                        parameters = evalResult;
                                    }
                                    else
                                    {
                                        foreach (string p in SplitTopLevelCommas(parameters))
                                        {
                                            object? value = unpackvar(p, variables, localFunctions, obj, mapObjects, mapcolors);
                                            if (value != null)
                                            {
                                                parameters = parameters.Replace(p, value.ToString()!);
                                            }
                                        }
                                    }
                                }
                            }
                            MessageBox.Show("Вывод: "+parameters);
                            break;
                        default:
                            {
                                FunctionDef? func = localFunctions.FirstOrDefault(f => f.name == command);
                                if (func != null)
                                {
                                    var args = parameters.Split(',').Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
                                    if (args.Count == func.parameters.Count)
                                    {
                                        object? result = CallFunction(func, args, variables, localFunctions, obj, mapObjects, mapcolors);
                                    }
                                }
                            }
                            break;
                    }
                    
                    Application.DoEvents();
                    Thread.Sleep(200);
                    
                    previousCommandEnd = script.IndexOf(';', end);
                    if (previousCommandEnd == -1) previousCommandEnd = script.Length;
                    previousCommand = command;
                    if (hasReturn)
                        break;
                }
                return returnValue;
            }
    }
}
