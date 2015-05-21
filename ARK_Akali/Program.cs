using System;
using System.CodeDom;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ARK_Akali.Properties;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;
using Color = System.Drawing.Color;

namespace ARK_Akali
{
    internal class Program
    {
        public const string ChampName = "Akali";
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;
        private static SpellSlot Ignite;
        public static HpBarIndicator Hpi = new HpBarIndicator();
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }
        

        private static void OnLoad(EventArgs args)
        {
            if (Player.ChampionName != ChampName)
                return;

            Notifications.AddNotification("ARK AKALI V1.124");

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 300);
            R = new Spell(SpellSlot.R, 700);

            Q.SetTargetted(Q.Instance.SData.SpellCastTime, Q.Instance.SData.MissileSpeed);
            E.SetSkillshot(E.Instance.SData.SpellCastTime, E.Instance.SData.LineWidth, E.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);

            Config = new Menu("ARK Akali", "Akali", true);
            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("[ARK]: Orbwalker", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("[ARK]: Target Selector", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("[ARK]: Combo Settings", "Combo Settings"));
            var harass = Config.AddSubMenu(new Menu("[ARK]: Harass Settings", "Harass Settings"));
            var killsteal = Config.AddSubMenu(new Menu("[ARK]: Killsteal Settings", "Killsteal Settings"));
            var laneclear = Config.AddSubMenu(new Menu("[ARK]: Laneclear Settings", "Laneclear Settings"));
            var lasthit = Config.AddSubMenu(new Menu("[ARK]: Lasthit Settings", "Lasthit Settings"));
            var jungleclear = Config.AddSubMenu(new Menu("[ARK]: Jungle Settings", "Jungle Settings"));
            var misc = Config.AddSubMenu(new Menu("[ARK]: Misc Settings", "Misc Settings"));
            var drawing = Config.AddSubMenu(new Menu("[ARK]: Draw Settings", "Draw Settings"));

            //Advanced Settings //[E] Settings
            combo.SubMenu("Advanced Features [Q/E]").AddItem(new MenuItem("prioE", "Use [E] after AA").SetValue(false));

            //[W] Settings
            combo.SubMenu("Advanced Features [W]").AddItem(new MenuItem("WPROC", "Use [W] on Dangerous Spells").SetValue(true));
            combo.SubMenu("Advanced Features [W]").AddItem(new MenuItem("WHP", "Use [W] on < % HP ").SetValue(true));
            combo.SubMenu("Advanced Features [W]").AddItem(new MenuItem("WHPslider", "% HP").SetValue(new Slider(35, 100, 0)));
            combo.SubMenu("Advanced Features [W]").AddItem(new MenuItem("WHE", "Use [W] X amount of enemies ").SetValue(true));
            combo.SubMenu("Advanced Features [W]").AddItem(new MenuItem("WHEslider", "Enemy Count").SetValue(new Slider(3, 5, 0)));

            //[R] Settings
            combo.SubMenu("Advanced Features [R]").AddItem(new MenuItem("Rkill", "Only use [R] if Killable").SetValue(true));
            combo.SubMenu("Advanced Features [R]").AddItem(new MenuItem("rturret", "Don't R into Turret Range if HP below %").SetValue(true));
            combo.SubMenu("Advanced Features [R]").AddItem(new MenuItem("rturrethp", "% HP").SetValue(new Slider(70, 100, 0)));
            combo.SubMenu("Advanced Features [R]").AddItem(new MenuItem("Rcheck", "Don't [R] into X amount of enemies").SetValue(false));
            combo.SubMenu("Advanced Features [R]").AddItem(new MenuItem("eslider", "Enemy Count").SetValue(new Slider(3, 5, 0)));
            combo.SubMenu("Advanced Features [R]").AddItem(new MenuItem("miniongapclose", "Use Minion to gapclose? [Requires 2 R stacks]").SetValue(true));
            combo.SubMenu("Advanced Features [R]").AddItem(new MenuItem("rangeRslider", "[R] gapclose Range").SetValue(new Slider(200, 600, 0)));

            //ITEMS
            combo.SubMenu("Item Settings").AddItem(new MenuItem("UseItems", "Use Offensive Items").SetValue(true));
            combo.SubMenu("Item Settings").AddItem(new MenuItem("UseBOTRK", "Use Blade of the Ruined King").SetValue(true));
            combo.SubMenu("Item Settings").AddItem(new MenuItem("eL", "  Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            combo.SubMenu("Item Settings").AddItem(new MenuItem("oL", "  Own HP Percentage").SetValue(new Slider(65, 100, 0)));
            combo.SubMenu("Item Settings").AddItem(new MenuItem("UseHEX", "Use Hextech Gunblade").SetValue(true));
            combo.SubMenu("Item Settings").AddItem(new MenuItem("eH", "  Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            combo.SubMenu("Item Settings").AddItem(new MenuItem("UseBILGE", "Use Bilgewater Cutlass").SetValue(true));
            combo.SubMenu("Item Settings").AddItem(new MenuItem("HLe", "  Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            combo.SubMenu("Item Settings").AddItem(new MenuItem("UseTIAMAT", "Use Tiamat").SetValue(true));
            combo.SubMenu("Item Settings").AddItem(new MenuItem("UseHYDRA", "Use Hydra").SetValue(true));

            //Combo Settings
            combo.AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            combo.AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            combo.AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            combo.AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));
            combo.AddItem(new MenuItem("UseF", "Use [Q-FLASH-AA-E-IGNITE] if Killable").SetValue(false));

            //LASTHIT
            lasthit.AddItem(new MenuItem("LastHitQ", "Lasthit with Q").SetValue(true));
            //DRAWING
            drawing.AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            drawing.SubMenu("Misc Drawings").AddItem(new MenuItem("drawdmg", "Draw Damage on Enemy HPbar").SetValue(true));
            drawing.SubMenu("Misc Drawings").AddItem(new MenuItem("drawpotential", "Draw Potential Combo DMG").SetValue(true));
            drawing.SubMenu("Misc Drawings").AddItem(new MenuItem("drawRend", "Draw R stacks").SetValue(true));
            drawing.SubMenu("Misc Drawings").AddItem(new MenuItem("rlines", "Draw [R] Gapclose Lines").SetValue(true));
            drawing.SubMenu("Misc Drawings").AddItem(new MenuItem("drawRstacks", "Draw R End Position").SetValue(true));
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("Qdraw", "Draw Q Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("Wdraw", "Draw W Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("Edraw", "Draw E Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("Rdraw", "Draw R Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("RGdraw", "Draw R Gapclose Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("CircleThickness", "Circle Thickness").SetValue(new Slider(7, 30, 0)));

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;
            Drawing.OnDraw += Gapcloserdraw;


        }

        private static void OnEndScene(EventArgs args)
        {
            if (Config.Item("drawdmg").GetValue<bool>())
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
                {
                    Hpi.unit = enemy;
                    Hpi.drawDmg(CalcDamage(enemy), System.Drawing.Color.IndianRed);

                }
            }
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            //Do stuff based on Orbwalker-Mode          
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo: //IF PLAYER WHOULD ENTER COMBO ORBWALKER MODE IT WILL USE SAID COMBO
                    Clogic();
                    Elogic();
                    Rlogic();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Lasthit();
                    break;

            }

            var zpos = Drawing.WorldToScreen(Player.Position);
            var rstacks = Player.Buffs.Find(buff => buff.Name == "AkaliShadowDance").Count;
            if (Config.Item("drawRstacks").GetValue<bool>())
                Drawing.DrawText(zpos.X - 50, zpos.Y + 50, System.Drawing.Color.HotPink,
                "[R] stacks = " + rstacks.ToString());

            if (Config.Item("UseItems").GetValue<bool>())
                Item();

        }
        
        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) 
            //W ON DANGEROUS SPELLS! SUCH AS RIVEN W/JAX Q/VLAD ULT/CAIT R/CASSIO R/RENGAR JUMP/LISS ULT/RYZE W/TALON E/ZED ULT/VAYNE E/VI ULT
        {
            throw new NotImplementedException();
        }

        //IGNITEDAMAGE
        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
                return 0f;
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        private static int CalcDamage(Obj_AI_Base target)
        {
            //Calculate Combo Damage
            var aa = Player.GetAutoAttackDamage(target, true);
            var damage = aa;
            var markdmg = Player.CalcDamage(target, Damage.DamageType.Magical,
                (45 + 35*Q.Level + 0.5*Player.FlatMagicDamageMod));

            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += Player.GetItemDamage(target, Damage.DamageItems.Botrk); //Botrk
            if (Items.HasItem(3077) && Items.CanUseItem(3077))
                damage += Player.GetItemDamage(target, Damage.DamageItems.Tiamat); //Tiamat
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += Player.GetItemDamage(target, Damage.DamageItems.Bilgewater); //Bigle
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
                damage += Player.GetItemDamage(target, Damage.DamageItems.Hydra); //Hydra
            if (Items.HasItem(3144) && Items.CanUseItem(3146))
                damage += Player.GetItemDamage(target, Damage.DamageItems.Hexgun); //Hexblade

            if (E.IsReady())
                damage += E.GetDamage(target);

            if (E.IsReady() && E.Level >= 5)
                damage += E.GetDamage(target) * 3;

            if (E.IsReady() && E.Level == Q.Level && Player.Level > 13)
                damage += E.GetDamage(target) * 3;
                           
            if (R.IsReady()) // rdamage          
                damage += R.GetDamage(target);

            if (target.HasBuff("AkaliMota"))
                damage += markdmg;

            if (Q.IsReady() && !target.HasBuff("AkaliMota"))           
                damage += Q.GetDamage(target) + markdmg;

            if (Q.IsReady() && target.HasBuff("AkaliMota"))
                damage += Q.GetDamage(target);

            if (Ignite.IsReady())
                damage += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            
            return (int) damage;
        }
        private static int PotentialDmg (Obj_AI_Hero target)
        {

            var aa = Player.GetAutoAttackDamage(target, true);
            var damage = aa;
            var markdmg = Player.CalcDamage(target, Damage.DamageType.Magical,
                (45 + 35 * Q.Level + 0.5 * Player.FlatMagicDamageMod));

            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += Player.GetItemDamage(target, Damage.DamageItems.Botrk); //Botrk
            if (Items.HasItem(3077) && Items.CanUseItem(3077))
                damage += Player.GetItemDamage(target, Damage.DamageItems.Tiamat); //Tiamat
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += Player.GetItemDamage(target, Damage.DamageItems.Bilgewater); //Bigle
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
                damage += Player.GetItemDamage(target, Damage.DamageItems.Hydra); //Hydra
            if (Items.HasItem(3144) && Items.CanUseItem(3146))
                damage += Player.GetItemDamage(target, Damage.DamageItems.Hexgun); //Hexblade

            if (E.Level >= 1)
                damage += E.GetDamage(target);

            if (E.Level >= 1 && E.Level >= 5)
                damage += E.GetDamage(target) * 3;

            if (E.Level >= 1 && E.Level == Q.Level && Player.Level > 13)
                damage += E.GetDamage(target) * 3;

            if (R.Level >= 1)  // rdamage          
                damage += R.GetDamage(target);

            if (Q.Level >= 1 && !target.HasBuff("AkaliMota"))
                damage += Q.GetDamage(target) + markdmg;


            if (Ignite.IsReady())
                damage += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);


            return (int)damage;


        }

        private static void Lasthit()
        {
            //LASTHIT
            foreach ( var minion in
                    ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget() && minion.IsEnemy &&
                                                                       minion.Distance(Player.ServerPosition) <= Q.Range))
            {
                float predictedHealthminionq = HealthPrediction.GetHealthPrediction(minion,
                (int)(Q.Delay + (Player.Distance(minion.ServerPosition) / Q.Speed)));

                if (Config.Item("LastHitQ").GetValue<bool>() && predictedHealthminionq < Q.GetDamage(minion))
                    Q.Cast(minion);
            }
           
        }

        private static void Rlogic()
        {
            var target = TargetSelector.GetTarget(R.Range * 2 + 150, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;
            var edmg = E.GetDamage(target);
            var rdmg = R.GetDamage(target);
            var qdmg = Q.GetDamage(target);
            var thp = target.Health;
            var RendPos = Player.ServerPosition.Extend(target.Position, Player.ServerPosition.Distance(target.Position) + 175);
            Ignite = Player.GetSpellSlot("summonerdot");
            
            var rrange = Config.Item("rangeRslider").GetValue<Slider>().Value;


            //TURRET CHECK

            if (R.IsReady() && Config.Item("UseR").GetValue<bool>())
            {
                if (target.CountEnemiesInRange(1200) > Config.Item("eslider").GetValue<Slider>().Value &&
                    Config.Item("Rcheck").GetValue<bool>())
                    return;

                if (RendPos.UnderTurret(true) && Config.Item("rturret").GetValue<bool>() &&
                    Player.HealthPercent < Config.Item("rturrethp").GetValue<Slider>().Value)
                    return;


                //When not to use R
                if (Player.Distance(target.ServerPosition) <= Orbwalking.GetRealAutoAttackRange(Player) && Player.HealthPercent > 30 && thp > rdmg)
                    return;
                if (Player.Distance(target.ServerPosition) <= E.Width - 15 && E.IsReady() && Player.HealthPercent > 40 && thp > edmg)
                    return;
                if (Player.Distance(target.ServerPosition) <= Q.Range && Q.IsReady() && thp < qdmg)
                    return;
                if (Player.Distance(target.ServerPosition) <= Orbwalking.GetRealAutoAttackRange(Player))
                    return;
                if (Player.Distance(target.Position) <= 600 && target.HasBuff("summonerdot") &&
                    target.Health < IgniteDamage(target))
                    return;

                //When to use R
                if (thp < PotentialDmg(target) + 15 * Player.Level)                  
                {
                        if (Config.Item("UseR").GetValue<bool>())
                        {
                            R.Cast(target, true);
                        }

                        //GAPCLOSE
                        if (Player.Distance(target.ServerPosition) >= rrange)

                            R.Cast(target, true);

                        else if (Player.Distance(target.ServerPosition) >= rrange)
                            R.Cast(target);

                        Gapcloser();

                }
            }
            
            //IF RKILL DISABLED

            //Turret check for enemy count
                    if (!Config.Item("Rkill").GetValue<bool>() && Player.Distance(target.ServerPosition) >= rrange)
                    
                        R.Cast(target, true);

                    else if (!Config.Item("Rkill").GetValue<bool>() && Player.Distance(target.ServerPosition) >= rrange)
                        R.Cast(target);

                    Gapcloser();


        }
        private static void Gapcloserdraw(EventArgs args)
        {
            var rstacks = Player.Buffs.Find(buff => buff.Name == "AkaliShadowDance").Count;
            var target = TargetSelector.GetTarget(R.Range * 2 + 300, TargetSelector.DamageType.Magical);


            foreach (var minion in
                ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget() && minion.IsEnemy &&
                                                                   minion.Distance(Player.ServerPosition) <=
                                                                   R.Range * 2))
            {
                if (Config.Item("miniongapclose").GetValue<bool>() && rstacks > 1 &&
                    minion.Distance(target.Position) > 100 &&
                    minion.Distance(target.Position) < R.Range && Player.Distance(target.Position) >= R.Range && minion.IsValidTarget(R.Range))
                {
                    var rdraw = new Geometry.Polygon.Line(Player.Position, minion.Position);
                    var rmdraw = new Geometry.Polygon.Line(minion.Position, target.Position);
                    rdraw.Draw(Color.White, 1);
                    rmdraw.Draw(Color.White, 1);
                }

                foreach (var h in
                    ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget() && h.IsEnemy &&
                                                                h.Distance(Player.ServerPosition) <=
                                                                R.Range * 2))
                {
                    if (Config.Item("miniongapclose").GetValue<bool>() && rstacks > 1 &&
                        Player.Distance(target.Position) >= R.Range && h.Distance(target.Position) > 100 && h.Distance(target.Position) < R.Range && h.IsValidTarget(R.Range))
                    {
                        var rdraw = new Geometry.Polygon.Line(Player.Position, h.Position);
                        var rmdraw = new Geometry.Polygon.Line(h.Position, target.Position);
                        rdraw.Draw(Color.White, 1);
                        rmdraw.Draw(Color.White, 1);
                    }
                }
            }
        }
        private static void Gapcloser()
        {
            var rstacks = Player.Buffs.Find(buff => buff.Name == "AkaliShadowDance").Count;
            var target = TargetSelector.GetTarget(R.Range*2 + 300, TargetSelector.DamageType.Magical);


            foreach (var minion in
                ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget() && minion.IsEnemy &&
                                                                   minion.Distance(Player.ServerPosition) <=
                                                                   R.Range*2))
            {
                if (Config.Item("miniongapclose").GetValue<bool>() && rstacks > 1 && minion.Distance(target.Position) > 100 &&
                    minion.Distance(target.Position) < R.Range && Player.Distance(target.Position) >= R.Range)
                    R.Cast(minion);

                 }
            foreach (var h in
                ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget() && h.IsEnemy &&
                                                            h.Distance(Player.ServerPosition) <=
                                                            R.Range*2))
            {
                if (Config.Item("miniongapclose").GetValue<bool>() && rstacks > 1 &&
                    Player.Distance(target.Position) >= R.Range && h.Distance(target.Position) > 100 &&
                    h.Distance(target.Position) < R.Range)
                    R.Cast(h);
            }
        }

        private static void Elogic()
        {
            Ignite = Player.GetSpellSlot("summonerdot");
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;
            if (E.IsReady() && Player.Distance(target.ServerPosition) >= Orbwalking.GetRealAutoAttackRange(Player) &&
                Config.Item("UseE").GetValue<bool>() && target.IsValidTarget(E.Range))
                E.Cast(target);

            //DONT USE E CANCEL -> IF KILLABLE
            if (E.IsReady() && target.Health < E.GetDamage(target) && target.Distance(Player.Position) < E.Width - 15)
                E.Cast(target, true);

            if (E.IsReady() && target.Health < E.GetDamage(target) + IgniteDamage(target) &&
                Config.Item("UseE").GetValue<bool>() && Config.Item("UseIgnite").GetValue<bool>()
                && target.Distance(Player.Position) < E.Width - 15)
            {
                E.Cast(target, true);
                Player.Spellbook.CastSpell(Ignite, target);
            }
        }

        private static void Item()
        {
            Ignite = Player.GetSpellSlot("summonerdot");
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();
            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var hextech = ItemData.Hextech_Gunblade.GetItem();
            if (target == null || !target.IsValidTarget())
                return;
            if (botrk.IsReady() && botrk.IsOwned(Player) && botrk.IsInRange(target)
            && target.HealthPercent <= Config.Item("eL").GetValue<Slider>().Value
            && Config.Item("UseBOTRK").GetValue<bool>())

                botrk.Cast(target);

            if (botrk.IsReady() && botrk.IsOwned(Player) && botrk.IsInRange(target)
                && Player.HealthPercent <= Config.Item("oL").GetValue<Slider>().Value
                && Config.Item("UseBOTRK").GetValue<bool>())

                botrk.Cast(target);

            if (cutlass.IsReady() && cutlass.IsOwned(Player) && cutlass.IsInRange(target) &&
                target.HealthPercent <= Config.Item("HLe").GetValue<Slider>().Value
                && Config.Item("UseBILGE").GetValue<bool>())

                cutlass.Cast(target);

            if (hextech.IsReady() && hextech.IsOwned(Player) && hextech.IsInRange(target) &&
                target.HealthPercent <= Config.Item("eH").GetValue<Slider>().Value
                && Config.Item("UseHEX").GetValue<bool>())

                hextech.Cast(target);

            if (Config.Item("UseIgnite").GetValue<bool>() && target.Health < IgniteDamage(target))
                Player.Spellbook.CastSpell(Ignite, target);
        }
        //COMBOLOGIC
        private static void Clogic()
        {
            Ignite = Player.GetSpellSlot("summonerdot");
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;
            var markdmg = Player.CalcDamage(target, Damage.DamageType.Magical,
                (45 + 35*Q.Level + 0.5*Player.FlatMagicDamageMod));          
            //Q BASIC CAST
            if (Q.IsReady() && target.IsValidTarget() && Config.Item("UseQ").GetValue<bool>())
                Q.Cast(target, true);

            //Q-E-IGNITE FINISH
            if (Config.Item("UseQ").GetValue<bool>() && Config.Item("UseE").GetValue<bool>() &&
                Config.Item("UseIgnite").GetValue<bool>()
                && Q.IsReady() && E.IsReady() &&
                target.Health < Q.GetDamage(target) + E.GetDamage(target) + IgniteDamage(target) + markdmg
                && target.Distance(Player.Position) <= Orbwalking.GetRealAutoAttackRange(Player))
            {
                Q.Cast(target, true);
                Player.Spellbook.CastSpell(Ignite, target);
            }

    }
        private static void OnReceiveBuff() //W on Cait R - Zed R
        {

            if (Player.HasBuff("zedultmark") || (Player.HasBuff("caitultbuff")))
                W.Cast();
        }
        private static void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            //E USAGE (LOWCD BETTER AFTER AA)
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;

            if (unit.IsMe && !Q.IsReady() && Config.Item("UseE").GetValue<bool>() && target.IsValidTarget(E.Range))
                E.Cast();


            var tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            var hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();

            if (unit.IsMe && hydra.IsReady() && Config.Item("UseHYDRA").GetValue<bool>() && target.IsValidTarget(hydra.Range))
                hydra.Cast();
            
            if (unit.IsMe && tiamat.IsReady() && Config.Item("UseTIAMAT").GetValue<bool>() && target.IsValidTarget(tiamat.Range))
                tiamat.Cast();
        }

        private static void OnDraw(EventArgs args)
        {

            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;
            //DRAW SPELL RANGES
                if (Config.Item("Qdraw").GetValue<Circle>().Active)
                    if (Q.Level > 0)
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range,
                            Q.IsReady() ? Config.Item("Qdraw").GetValue<Circle>().Color : System.Drawing.Color.Red,
                                                        Config.Item("CircleThickness").GetValue<Slider>().Value); //Thickness

                if (Config.Item("Wdraw").GetValue<Circle>().Active)
                    if (W.Level > 0)
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range,
                            W.IsReady() ? Config.Item("Wdraw").GetValue<Circle>().Color : System.Drawing.Color.Red,
                                                        Config.Item("CircleThickness").GetValue<Slider>().Value); //Thickness

                if (Config.Item("Edraw").GetValue<Circle>().Active)
                    if (E.Level > 0)
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range,
                            E.IsReady() ? Config.Item("Edraw").GetValue<Circle>().Color : System.Drawing.Color.Red,
                                                        Config.Item("CircleThickness").GetValue<Slider>().Value); //Thickness

                if (Config.Item("Rdraw").GetValue<Circle>().Active)
                    if (R.Level > 0)
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range,
                            R.IsReady() ? Config.Item("Rdraw").GetValue<Circle>().Color : System.Drawing.Color.Red,
                                                            Config.Item("CircleThickness").GetValue<Slider>().Value); //Thickness

                if (Config.Item("RGdraw").GetValue<Circle>().Active)
                    if (R.Level >= 0)
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, Config.Item("rangeRslider").GetValue<Slider>().Value,
                            R.IsReady() ? Config.Item("RGdraw").GetValue<Circle>().Color : System.Drawing.Color.Red,
                                                            Config.Item("CircleThickness").GetValue<Slider>().Value); //Thickness

                //Draw orbwalker target
                var orbwalkert = Orbwalker.GetTarget();
                if (orbwalkert.IsValidTarget(R.Range))
                    Render.Circle.DrawCircle(orbwalkert.Position, 50, System.Drawing.Color.IndianRed, 7);


               //Draw Killable minion AA
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget() && x.IsEnemy
                                                                                     && x.Distance(Player.ServerPosition) <
                                                                                     E.Range &&
                                                                                     x.Health < Player.GetAutoAttackDamage(x)))
                {
                    Render.Circle.DrawCircle(minion.Position, 50, System.Drawing.Color.IndianRed, 10);
                }

            //DRAW LINES BETWEEN MINION AND TARGET

            //DRAW POTENTIAL DMG IN NUMBERS
            foreach (var enemy in
                ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
            {
                var epos = Drawing.WorldToScreen(enemy.Position);
                if (Config.Item("drawpotential").GetValue<bool>())
                    Drawing.DrawText(epos.X - 50, epos.Y + 50, System.Drawing.Color.Gold,
                        "Potential DMG = " + PotentialDmg(enemy).ToString());
            }


            var target = TargetSelector.GetTarget(R.Range*2 + 500, TargetSelector.DamageType.Magical);
            var RendPos = Player.ServerPosition.Extend(target.Position, Player.ServerPosition.Distance(target.Position) + 175);
            if (Config.Item("drawRend").GetValue<bool>() && !target.IsMoving)
                  Render.Circle.DrawCircle(RendPos, 15, System.Drawing.Color.Green, 20);
            if (Config.Item("drawRend").GetValue<bool>() && target.IsMoving && !target.IsFacing(Player))
                Render.Circle.DrawCircle(RendPos - 75, 15, System.Drawing.Color.Green, 20);






        }

        }      
}
    

