// ⓅⓈⒾ  ●  Pascal Language System  ●  Academy'23
// Parser.cs ~ Recursive descent parser for Pascal Grammar
// ─────────────────────────────────────────────────────────────────────────────
namespace PSI;
using static Token.E;
using static NType;
using System.Collections.Generic;

public class Parser {
   // Interface -------------------------------------------
   public Parser (Tokenizer tokenizer)
      => mToken = mPrevPrev = mPrevious = (mTokenizer = tokenizer).Next ();

   public NProgram Parse () {
      var node = Program ();
      if (mToken.Kind != EOF) Unexpected ();
      return node;
   }

   #region Declarations ------------------------------------
   // program = "program" IDENT ";" block "." .
   NProgram Program () {
      Expect (PROGRAM); var name = Expect (IDENT); Expect (SEMI);
      var block = Block (); Expect (PERIOD);
      return new (name, block);
   }

   // block = declarations compound-stmt .
   NBlock Block ()
      => new (Declarations (), CompoundStmt ());

   // declarations = [var-decls] [procfn-decls] .
   NDeclarations Declarations () {
      List<NVarDecl> vars = new ();
      if (Match (VAR)) 
         do { vars.AddRange (VarDecls ()); Expect (SEMI); } while (Peek (IDENT));
      List<NProcFnDecl> fns = new ();
      while (Peek (PROCEDURE) || Peek (FUNCTION)) { fns.Add (ProcFnDecl ()); }
      return new (vars.ToArray (), fns.ToArray ());
   }

   // ident-list = IDENT { "," IDENT }
   Token[] IdentList () {
      List<Token> names = new ();
      do { names.Add (Expect (IDENT)); } while (Match (COMMA));
      return names.ToArray (); 
   }

   // procfn-decls = { proc-decl | func-decl }
   NProcFnDecl ProcFnDecl () {
      if (Match (PROCEDURE)) return new (ProcDecl (), null!);
      else if (Match (FUNCTION)) return new (null!, FuncDecl ());
      else return null!;
   }

   // var-decl = ident-list ":" type
   NVarDecl[] VarDecls () {
      var names = IdentList (); Expect (COLON); var type = Type ();
      return names.Select (a => new NVarDecl (a, type)).ToArray ();
   }

   // proc-decl = "procedure" IDENT paramlist; block ";" .
   NProcDecl ProcDecl () {
      var (name, param) = (Expect (IDENT), ParamList ());
      Expect (SEMI);
      return new (name, param, Block ());
   }

   // func-decl = "function" IDENT paramlist ":" type; block ";" .
   NFuncDecl FuncDecl () {
      var (name, param) = (Expect (IDENT), ParamList ());
      Expect (COLON); var type = Type (); Expect (SEMI);
      return new (name, param, type, Block ());
   }

   // paramlist = "(" var-decl { "," var-decl } ")"
   NParamList ParamList () {
      Expect (OPEN);
      List<NVarDecl> list = new ();
      while (!Match (CLOSE)) {
         list.AddRange (VarDecls ());
         Match (COMMA);
      }
      return new (list.ToArray ());
   }

   // type = integer | real | boolean | string | char
   NType Type () {
      var token = Expect (INTEGER, REAL, BOOLEAN, STRING, CHAR);
      return token.Kind switch {
         INTEGER => Int, REAL => Real, BOOLEAN => Bool, 
         STRING => String, _ => Char,
      };
   }
   #endregion
   
   #region Statements ---------------------------------------
   // statement         =  write-stmt | read-stmt | assign-stmt | call-stmt |
   //                      goto-stmt | if-stmt | while-stmt | repeat-stmt |
   //                      compound-stmt | for-stmt | case-stmt
   NStmt Stmt () {
      if (Match (WRITE, WRITELN)) return WriteStmt ();
      if (Match (READ)) return ReadStmt ();
      if (Match (IDENT)) {
         if (Match (ASSIGN)) return AssignStmt ();
         else return CallStmt ();
      }
      if (Match (IF)) return IfStmt ();
      if (Match (WHILE)) return WhileStmt ();
      if (Match (REPEAT)) return RepeatStmt ();
      if (Match (FOR)) return ForStmt ();
      return null!;
   }

   // compound-stmt = "begin" [ statement { ";" statement } ] "end" .
   NCompoundStmt CompoundStmt () {
      Expect (BEGIN);
      List<NStmt> stmts = new ();
      while (!Match (END)) { stmts.Add (Stmt ()); Match (SEMI); }
      Match (SEMI);
      return new (stmts.ToArray ());
   }

   // write-stmt =  ( "writeln" | "write" ) arglist .
   NWriteStmt WriteStmt () 
      => new (Prev.Kind == WRITELN, ArgList ());

   // read-stmt = "read" "(" identlist ")" .
   NReadStmt ReadStmt () {
      Expect (OPEN); Expect (IDENT); Expect (CLOSE);
      return new (Prev);
   }

   // assign-stmt = IDENT ":=" expr .
   NAssignStmt AssignStmt () 
      => new (PrevPrev, Expression ());

   // call-stmt =  IDENT arglist .
   NCallStmt CallStmt ()
      => new (Prev, ArgList ());

   // if-stmt =  "if" expression "then" statement [ "else" statement ] .
   NIfStmt IfStmt () {
      var ifExpr = Expression ();
      Expect (THEN);
      List<NStmt> thenStmts = new (), elseStmts = new ();
      if (Match (BEGIN))
         do { thenStmts.Add (Stmt ()); Match (SEMI); } while (!Match (END));
      else thenStmts.Add (Stmt ());
      Match (SEMI);
      if (Match (ELSE))
         if (Match (BEGIN))
            do { elseStmts.Add (Stmt ()); Match (SEMI); } while (!Match (END));
         else elseStmts.Add (Stmt ());
      return new NIfStmt (ifExpr, new (thenStmts.ToArray ()), new (thenStmts.ToArray ()));
   }

   // while-stmt =  "while" expression "do" statement .
   NWhileStmt WhileStmt () {
      var expr = Expression ();
      Expect (DO);
      List<NStmt> stmts = new ();
      if (Match (BEGIN))
         do { stmts.Add (Stmt ()); Match (SEMI); } while (!Match (END));
      else stmts.Add (Stmt ());
      return new (expr, new (stmts.ToArray ()));
   }

   // repeat-stmt =  "repeat" statement { ";" statement} "until" expression. 
   NRepeatStmt RepeatStmt () {
      List<NStmt> stmts = new ();
      while (!Match (UNTIL)) { stmts.Add (Stmt ()); Match (SEMI); }
      var expr = Expression ();
      Expect (SEMI);
      return new (expr, stmts.ToArray ());
   }

   // for-stmt =  "for" IDENT ":=" expression ( "to" | "downto" ) expression "do" statement.
   NForStmt ForStmt () {
      var ident = Expect (IDENT);
      Expect (ASSIGN); var from = Expression ();
      Expect (TO, DOWNTO); var to = Expression ();
      Expect (DO);
      List<NStmt> stmts = new ();
      if (Match (BEGIN))
         do { stmts.Add (Stmt ()); Match (SEMI); } while (!Match (END));
      else stmts.Add (Stmt ());
      return new (ident, from, to, stmts.ToArray ());
   }
   #endregion

   #region Expression --------------------------------------
   // expression = equality .
   NExpr Expression () 
      => Equality ();

   // equality = equality = comparison [ ("=" | "<>") comparison ] .
   NExpr Equality () {
      var expr = Comparison ();
      if (Match (EQ, NEQ)) 
         expr = new NBinary (expr, Prev, Comparison ());
      return expr;
   }

   // comparison = term [ ("<" | "<=" | ">" | ">=") term ] .
   NExpr Comparison () {
      var expr = Term ();
      if (Match (LT, LEQ, GT, GEQ))
         expr = new NBinary (expr, Prev, Term ());
      return expr;
   }

   // term = factor { ( "+" | "-" | "or" ) factor } .
   NExpr Term () {
      var expr = Factor ();
      while  (Match (ADD, SUB, OR)) 
         expr = new NBinary (expr, Prev, Factor ());
      return expr;
   }

   // factor = unary { ( "*" | "/" | "and" | "mod" ) unary } .
   NExpr Factor () {
      var expr = Unary ();
      while (Match (MUL, DIV, AND, MOD)) 
         expr = new NBinary (expr, Prev, Unary ());
      return expr;
   }

   // unary = ( "-" | "+" ) unary | primary .
   NExpr Unary () {
      if (Match (ADD, SUB))
         return new NUnary (Prev, Unary ());
      return Primary ();
   }

   // primary = IDENTIFIER | INTEGER | REAL | STRING | "(" expression ")" | "not" primary | IDENTIFIER arglist .
   NExpr Primary () {
      if (Match (IDENT)) {
         if (Peek (OPEN)) return new NFnCall (Prev, ArgList ());
         return new NIdentifier (Prev);
      }
      if (Match (L_INTEGER, L_REAL, L_BOOLEAN, L_CHAR, L_STRING)) return new NLiteral (Prev);
      if (Match (NOT)) return new NUnary (Prev, Primary ());
      Expect (OPEN, "Expecting identifier or literal");
      var expr = Expression ();
      Expect (CLOSE);
      return expr;
   }

   // arglist = "(" [ expression { , expression } ] ")"
   NExpr[] ArgList () {
      List<NExpr> args = new ();
      Expect (OPEN);
      if (!Peek (CLOSE)) args.Add (Expression ());
      while (Match (COMMA)) args.Add (Expression ());
      Expect (CLOSE);
      return args.ToArray ();
   }
   #endregion

   #region Helpers -----------------------------------------
   // Expect to find a particular token
   Token Expect (Token.E kind, string message) {
      if (!Match (kind)) Throw (message);
      return mPrevious;
   }

   Token Expect (params Token.E[] kinds) {
      if (!Match (kinds)) 
         Throw ($"Expecting {string.Join (" or ", kinds)}");
      return mPrevious;
   }

   // Like Match, but does not consume the token
   bool Peek (params Token.E[] kinds)
      => kinds.Contains (mToken.Kind);

   // Match and consume a token on match
   bool Match (params Token.E[] kinds) {
      if (kinds.Contains (mToken.Kind)) {
         mPrevPrev = mPrevious; mPrevious = mToken; 
         mToken = mTokenizer.Next ();
         return true;
      }
      return false;
   }

   [DoesNotReturn]
   void Throw (string message) {
      throw new ParseException (mTokenizer.FileName, mTokenizer.Lines, mToken.Line, mToken.Column, message);
   }

   [DoesNotReturn]
   void Unexpected () {
      string message = $"Unexpected {mToken}";
      if (mToken.Kind == ERROR) message = mToken.Text;
      Throw (message);
   }

   // The 'previous' two tokens we've seen
   Token Prev => mPrevious;
   Token PrevPrev => mPrevPrev;

   Token mToken, mPrevious, mPrevPrev;
   readonly Tokenizer mTokenizer;
   #endregion 
}