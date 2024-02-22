// ⓅⓈⒾ  ●  Pascal Language System  ●  Academy'23
// Node.cs ~ All the syntax tree Nodes
// ─────────────────────────────────────────────────────────────────────────────
namespace PSI;

// Base class for all program nodes
public abstract record Node {
   public abstract T Accept<T> (Visitor<T> visitor);
}

#region Main, Declarations -----------------------------------------------------
// The Program node (top node)
public record NProgram (Token Name, NBlock Block) : Node {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// A block contains declarations and a body
public record NBlock (NDeclarations Decls, NCompoundStmt Body) : Node {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// The declarations section precedes the body of every block
public record NDeclarations (NVarDecl[] Vars, NProcFnDecl[] ProcFunc) : Node {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// Identifier list
public record NIdentList (Token[] Names) : Node {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// List of variable declarations
public record NVarDecls (NVarDecl[] VarDecls) : Node {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// Declares a procedure or function
public record NProcFnDecl (NProcDecl Procedure, NFuncDecl Function) : Node {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// Declares a variable (with a type)
public record NVarDecl (Token Name, NType Type) : Node {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// Declares a Procedure
public record NProcDecl (Token Name, NParamList ParamList, NBlock Block) : Node {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// Declares a function
public record NFuncDecl (Token Name, NParamList ParamList, NType ReturnType, NBlock Block) : Node {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// Declares a parameter list
public record NParamList (NVarDecl[] VarDecls) : Node {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}
#endregion

#region Statements -------------------------------------------------------------
// Base class for various types of statements
public abstract record NStmt : Node { }

// A Write or WriteLn statement (NewLine differentiates between the two)
public record NWriteStmt (bool NewLine, NExpr[] Exprs) : NStmt {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// A read statement
public record NReadStmt (Token Name) : NStmt {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// An assignment statement
public record NAssignStmt (Token Name, NExpr Expr) : NStmt {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// A call statement
public record NCallStmt (Token Name, NExpr[] Exprs) : NStmt {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// An if statement
public record NIfStmt (NExpr If, NCompoundStmt Then, NCompoundStmt Else) : NStmt {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// A while statement
public record NWhileStmt (NExpr While, NCompoundStmt DoStmts) : NStmt {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// A repeat statement
public record NRepeatStmt (NExpr Until, NStmt[] DoStmts) : NStmt {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// A for statement
public record NForStmt (Token IdentName, NExpr FromExpr, NExpr ToExpr, NStmt[] DoStmt) : NStmt {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// A compound statement (begin { stmts }* end)
public record NCompoundStmt (NStmt[] Stmts) : NStmt {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}
#endregion

#region Expression nodes -------------------------------------------------------
// Base class for expression nodes
public abstract record NExpr : Node {
   public NType Type { get; set; }     // The type of this expression
}

// A Literal (string / real / integer /  ...)
public record NLiteral (Token Value) : NExpr {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// An identifier (type depends on symbol-table lookup)
public record NIdentifier (Token Name) : NExpr {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// Unary operator expression
public record NUnary (Token Op, NExpr Expr) : NExpr {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// Binary operator expression 
public record NBinary (NExpr Left, Token Op, NExpr Right) : NExpr {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}

// A function-call node in an expression
public record NFnCall (Token Name, NExpr[] Params) : NExpr {
   public override T Accept<T> (Visitor<T> visitor) => visitor.Visit (this);
}
#endregion
