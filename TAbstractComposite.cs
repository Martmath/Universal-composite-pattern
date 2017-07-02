using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace WindowsFormsApp3
{
    public class TObjectComposer : TAbstractComposite<object, TObjectComposer> {
      
            public object this[int index,string t] {
            get {
                return Element[index];
            }
            set {
                Element[index] = (TObjectComposer)value;
            }
        }

    }
    public enum  EnumerationType
    {
        FromUptoDown,
        FromDowntoUp,
        Flat
    }
    public abstract class TAbstractComposite<TParent, Tchild> where
            Tchild : TAbstractComposite<TParent, Tchild>
    {

   /*     public TObjectComposer Reorder(params Func<Tchild, object>[] D) {
            //List<TObjectComposer> CurrentLevel
            var list = this.ToList();
            var sorted = list.OrderBy(D[0]).ThenBy(D[1]).ThenBy(D[2]);
            sorted.
            foreach (var Me in this.ToList()) {
                

                foreach (var Me in this.ToList()) {

                }

                }

            Dictionary<object,List<Tchild>> 
            List<Tchild> Res = new List<Tchild>();
            DoItAlone doIt = new DoItAlone(new Action<Tchild>(me => { if (Delegate(me)) Res.Add(me); }));
            RunForeachChildren(doIt);
            return Res;
        }*/


        public delegate void DoIt(Tchild child, List<Tchild> ParC);
        public delegate void DoItAlone(Tchild child);
        public delegate void DoItParent(Tchild Parent, Tchild child);
        [XmlIgnore]
        public TParent Me;
        [XmlElement]
        public List<Tchild> Element;
        public static DoIt doIt;      
        
        public TAbstractComposite()
        {
            Init();
        }
        public TAbstractComposite(TParent parent) : this()
        {
            Me = parent;
        }
        public List<Tchild> ToList()
        {
            List<Tchild> Res = new List<Tchild>() {(Tchild)this};
            DoItAlone doIt = new DoItAlone(new Action<Tchild>(me => Res.Add(me)));
            RunForeachChildren(doIt);
            return Res;
        }
        public List<Tchild> Where(Func<Tchild, List<Tchild>,bool> Delegate)
        {
            List<Tchild> Res = new List<Tchild>();
            DoIt doIt = new DoIt(new Action<Tchild, List<Tchild>>((me, plist) =>
            {if (Delegate(me, plist)) Res.Add(me); }));
            if (Delegate((Tchild)this,new List<Tchild>())) Res.Add((Tchild)this);
            RunForeachChildren(doIt);
            return Res;
        }
        public List<Tchild> Where(Func<Tchild, bool> Delegate)
        {
            List<Tchild> Res = new List<Tchild>();
            DoItAlone doIt = new DoItAlone(new Action<Tchild>(me =>
            { if (Delegate(me)) Res.Add(me); }));
            if (Delegate((Tchild)this)) Res.Add((Tchild)this);
            RunForeachChildren(doIt);
            return Res;
        }
        public Tchild FirstorDefault(Func<Tchild, bool> Delegate)
        {      
            bool All = false;
            Tchild Res = default(Tchild);
            Action<Tchild> Do = null;
            Do = new Action<Tchild>(me => {
                if (Delegate (me))
                {
                    Res = me;
                    All = true;
                    return;
                }
                if (me != null)
                {
                    foreach (var localchild in me.Element)
                    {
                        Do(localchild); if (All) return;
                    }
                }
            });
            Do((Tchild)this);
            return Res;
        }
        public void RunForeachChildren(DoIt doIt, EnumerationType EnumerationType = EnumerationType.FromDowntoUp)
        {
            if (EnumerationType == EnumerationType.FromDowntoUp)
            {
                foreachChildrenDown(new List<Tchild>() { (Tchild)this }, doIt);
                doIt((Tchild)this, new List<Tchild>());
            }
            else if (EnumerationType == EnumerationType.FromUptoDown) {
                doIt((Tchild)this, new List<Tchild>());
                foreachChildrenUp(new List<Tchild>() { (Tchild)this }, doIt);
            }
            else
            {
                throw new ArgumentException();
            }
        }
        public void RunForeachChildren(DoItAlone doIt, EnumerationType EnumerationType = EnumerationType.FromDowntoUp)
        {
            if (EnumerationType == EnumerationType.FromDowntoUp)
            {
                foreachChildrenDown((Tchild)this, doIt);
                doIt((Tchild)this);
            }
            else if (EnumerationType == EnumerationType.FromUptoDown)
            {
                doIt((Tchild)this);
                foreachChildrenUp( (Tchild)this, doIt);
            }
            else if (EnumerationType == EnumerationType.Flat)
            {
                doIt((Tchild)this);
                foreachChildrenFlat(((Tchild)this).Element, doIt);
            }
            else
            {
                throw new ArgumentException();
            }
        }
        public void RunForeachChildren(DoItAlone doIt)
        {
            foreachChildrenDown((Tchild)this, doIt);
        }
        private void foreachChildrenFlat(List<Tchild> Parents, DoItAlone doIt) {       
            Element?.ForEach(localchild => {
                if (localchild.Me != null) doIt(localchild);                        
            });
            foreachChildrenFlat(Parents.SelectMany(x => x.Element).ToList(), doIt);
        }        
        private void foreachChildrenUp(Tchild Parents, DoItAlone doIt)
        {
            Parents?.Element?.ForEach(localchild =>
            {
                if (localchild.Me != null) doIt(localchild);
                foreachChildrenUp(localchild, doIt);                
            });
        }
        private void foreachChildrenUp(List<Tchild> Parents, DoIt doIt) {
            this.Element?.ForEach(localchild => {
                if ((localchild != null) &&(localchild.Me != null)) doIt(localchild, Parents);
                List<Tchild> parents = new List<Tchild>(Parents);
            parents.Add(localchild);
            localchild?.foreachChildrenUp(parents, doIt);            
            });
        }
        private void foreachChildrenDown(Tchild Parents, DoItAlone doIt)
        {
            Parents?.Element?.ForEach(localchild =>
            {
                foreachChildrenDown(localchild, doIt);
                if ((localchild != null) &&(localchild.Me != null)) doIt(localchild);
            });
        }
        private void foreachChildrenDown(List<Tchild> Parents, DoIt doIt)
        {
            Element?.ForEach(localchild =>
            {
                List<Tchild> parents = new List<Tchild>(Parents);
                parents.Add(localchild);
                localchild.foreachChildrenDown(parents, doIt);
                if (localchild.Me!=null)  doIt(localchild, Parents);
            });
        } 
        public virtual void Add(Tchild me)
        {
            Element.Add(me);
        }
        public int Count
        {
            get { return Element.Count; }
        }
        public virtual void Remove(Tchild me)
        {
            Element.Remove(me);
        }
        public virtual void Init()
        {
            Element = new List<Tchild>();//override toNothing do for Leaf
        }
        public Tchild this[int index]
        {
            get
            {
                return Element[index];
            }
            set
            {
                Element[index] = value;
            }
        }
        public static implicit operator TParent(TAbstractComposite<TParent, Tchild> Me) { return Me.Me; }
    }
}
