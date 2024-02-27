// ⓅⓈⒾ  ●  Pascal Language System  ●  Academy'23
// PSIPrint.cs ~ Prints a PSI syntax tree in Pascal format
// ─────────────────────────────────────────────────────────────────────────────
namespace PSI;

public class PSIPrint : Visitor<StringBuilder> {
   public override StringBuilder Visit (NProgram p) {
      Write ($"program {p.Name}; ");
      Visit (p.Block);
      return Write (".");
   }

   public override StringBuilder Visit (NBlock b) 
      => Visit (b.Decls, b.Body);

   public override StringBuilder Visit (NDeclarations d) {
      if (d.Vars.Length > 0) {
         NWrite ("var"); N++;
         foreach (var g in d.Vars.GroupBy (a => a.Type))
            NWrite ($"{g.Select (a => a.Name).ToCSV ()} : {g.Key};");
         N--;
      }
      d.ProcFunc.ForEach (a => a.Accept (this));
      return S;
   }

   public override StringBuilder Visit (NVarDecl d)
      => Write ($"{d.Name} : {d.Type}");

   public override StringBuilder Visit (NCompoundStmt b) {
      NWrite ("begin"); N++;  Visit (b.Stmts); N--; return NWrite ("end"); 
   }

   public override StringBuilder Visit (NAssignStmt a) {
      NWrite ($"{a.Name} := "); a.Expr.Accept (this); return Write (";");
   }

   public override StringBuilder Visit (NWriteStmt w) {
      NWrite (w.NewLine ? "WriteLn (" : "Write (");
      for (int i = 0; i < w.Exprs.Length; i++) {
         if (i > 0) Write (", ");
         w.Exprs[i].Accept (this);
      }
      return Write (");");
   }

   public override StringBuilder Visit (NLiteral t)
      => Write (t.Value.ToString ());

   public override StringBuilder Visit (NIdentifier d)
      => Write (d.Name.Text);

   public override StringBuilder Visit (NUnary u) {
      Write (u.Op.Text); return u.Expr.Accept (this);
   }

   public override StringBuilder Visit (NBinary b) {
      Write ("("); b.Left.Accept (this); Write ($" {b.Op.Text} ");
      b.Right.Accept (this); return Write (")");
   }

   public override StringBuilder Visit (NFnCall f) {
      Write ($"{f.Name} (");
      for (int i = 0; i < f.Params.Length; i++) {
         if (i > 0) Write (", "); f.Params[i].Accept (this);
      }
      return Write (")");
   }

   StringBuilder Visit (params Node[] nodes) {
      nodes.ForEach (a => a.Accept (this));
      return S;
   }

   // Writes in a new line
   StringBuilder NWrite (string txt) 
      => Write ($"\n{new string (' ', N * 3)}{txt}");
   int N;   // Indent level

   // Continue writing on the same line
   StringBuilder Write (string txt) {
      Console.Write (txt);
      S.Append (txt);
      return S;
   }

   public override StringBuilder Visit (NIdentList f) {
      Write ($"{f.Names.FirstOrDefault ()}");
      for (int i = 0; i < f.Names.Length; i++) {
         if (i > 0) Write (", "); Write ($"{f.Names[i]}");
      }
      return Write (";");
   }

   public override StringBuilder Visit (NVarDecls d) {
      Write ("var\n");
      d.VarDecls.ForEach (a => { a.Accept (this); Write (","); });
      return S;
   }

   public override StringBuilder Visit (NProcFnDecl d) {
      if (d.Function != null) {
         NWrite ($"function ");
         d.Function.Accept (this);
      } else if (d.Procedure != null) {
         NWrite ($"procedure ");
         d.Procedure.Accept (this);
      }
      return S;
   }

   public override StringBuilder Visit (NProcDecl proc) {
      Write ($"{proc.Name} ");
      proc.ParamList.Accept (this);
      Write (";");
      return proc.Block.Accept (this);
   }

   public override StringBuilder Visit (NFuncDecl func) {
      Write ($"{func.Name} ");
      func.ParamList.Accept (this);
      Write ($": {func.ReturnType};");
      return func.Block.Accept (this);
   }

   public override StringBuilder Visit (NParamList paramList) {
      Write ("(");
      for (int i = 0; i < paramList.VarDecls.Length; i++) {
         if (i > 0) Write (",");
         paramList.VarDecls[i].Accept (this);
      }
      return Write (")");
   }

   public override StringBuilder Visit (NReadStmt r)
      => NWrite ($"read ({r.Name})");

   public override StringBuilder Visit (NCallStmt cs) {
      NWrite ($"{cs.Name} (");
      cs.Exprs.ForEach (a => a.Accept (this));
      return Write (");");
   }

   public override StringBuilder Visit (NIfStmt i) {
      NWrite ("if ");
      i.If.Accept (this);
      Write (" then "); N++;
      i.Then.Accept (this); N--;
      if (i.Else != null) {
         NWrite ("else "); N++;
         i.Else.Accept (this); N--;
      }
      return S;
   }

   public override StringBuilder Visit (NWhileStmt w) {
      NWrite ("while ");
      w.While.Accept (this);
      Write (" do "); N++;
      w.DoStmt.Accept (this); N--;
      return S;
   }

   public override StringBuilder Visit (NRepeatStmt r) {
      NWrite ("repeat"); N++;
      r.DoStmts.ForEach (a => a.Accept (this)); N--;
      NWrite ("until "); r.Until.Accept (this); return Write (";");
   }

   public override StringBuilder Visit (NForStmt f) {
      NWrite ($"for {f.IdentName} := "); f.FromExpr.Accept (this);
      Write (" to "); f.ToExpr.Accept (this);
      Write (" do"); N++;
      f.DoStmt.Accept (this); N--;
      return S;
   }

   readonly StringBuilder S = new ();
}