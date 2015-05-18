using System;
using System.CodeDom;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;

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
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 700);

            Config = new Menu("ARK Akali", "Akali", true);
            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("[ARK]: Orbwalker", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("[ARK]: Target Selector", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("[ARK]: Combo Settings", "Combo Settings"));
            var harass = Config.AddSubMenu(new Menu("[ARK]: Harass Settings", "Harass Settings"));
            var killsteal = Config.AddSubMenu(new Menu("[ARK]: Killsteal Settings", "Killsteal Settings"));
            var laneclear = Config.AddSubMenu(new Menu("[ARK]: Laneclear Settings", "Laneclear Settings"));
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
            combo.SubMenu("Advanced Features [R]").AddItem(new MenuItem("rturretcheck", "Don't R into Turret Range if HP below %").SetValue(true));
            combo.SubMenu("Advanced Features [W]").AddItem(new MenuItem("turrethp", "% HP").SetValue(new Slider(70, 100, 0)));
            combo.SubMenu("Advanced Features [R]").AddItem(new MenuItem("Rcheck", "Don't [R] into X amount of enemies").SetValue(false));
            combo.SubMenu("Advanced Features [R]").AddItem(new MenuItem("eslider", "Enemy Count").SetValue(new Slider(3, 5, 0)));
            combo.SubMenu("Advanced Features [R]").AddItem(new MenuItem("rangeR", "Use Minion to gapclose? [Requires 2 R stacks]").SetValue(true));
            combo.SubMenu("Advanced Features [R]").AddItem(new MenuItem("rangeRslider", "[R] gapclose Range").SetValue(new Slider(200, 600, 0)));

            //ITEMS
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

            //DRAWING
            drawing.AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            drawing.AddItem(new MenuItem("drawdmg", "Draw Combo Damage").SetValue(true));
            drawing.AddItem(new MenuItem("Qdraw", "Draw Q Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.AddItem(new MenuItem("Wdraw", "Draw W Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.AddItem(new MenuItem("Edraw", "Draw E Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.AddItem(new MenuItem("Rdraw", "Draw R Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.AddItem(new MenuItem("RGdraw", "Draw R Gapclose Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.AddItem(new MenuItem("CircleThickness", "Circle Thickness").SetValue(new Slider(7, 30, 0)));

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;
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
                    Item();
                    break;
            }

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

        private static void Rlogic()
        {
            var target = TargetSelector.GetTarget(R.Range * 2 + 500, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;
            var markdmg = Player.CalcDamage(target, Damage.DamageType.Magical,
                (45 + 35*Q.Level + 0.5*Player.FlatMagicDamageMod) + Player.GetAutoAttackDamage(target));
            var edmg = E.GetDamage(target);
            var rdmg = R.GetDamage(target);
            var qdmg = Q.GetDamage(target);
            var hydradmg = Player.GetItemDamage(target, Damage.DamageItems.Hydra);
            var hexdmg = Player.GetItemDamage(target, Damage.DamageItems.Hexgun);
            var botrkdmg = Player.GetItemDamage(target, Damage.DamageItems.Botrk); //Botrk
            var tiamatdmg = Player.GetItemDamage(target, Damage.DamageItems.Tiamat); //Tiamat
            var bilgedmg =Player.GetItemDamage(target, Damage.DamageItems.Bilgewater); //Bigle
            var thp = target.Health;
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();
            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var hextech = ItemData.Hextech_Gunblade.GetItem();
            var tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            var hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
            var prediction = R.GetPrediction(target);
            var RendPos = Player.ServerPosition.Extend(prediction.CastPosition, Player.ServerPosition.Distance(prediction.CastPosition) + 150);
            Ignite = Player.GetSpellSlot("summonerdot");
            
            var rrange = Config.Item("rangeRslider").GetValue<Slider>().Value;

            if (R.IsReady())
            {
                //When not to use R
                if (Player.Distance(target.ServerPosition) <= Orbwalking.GetRealAutoAttackRange(Player) && Player.HealthPercent > 30 && thp > rdmg)
                    return;
                if (Player.Distance(target.ServerPosition) <= E.Range && E.IsReady() && Player.HealthPercent > 40 && thp < edmg)
                    return;
                if (Player.Distance(target.ServerPosition) <= Q.Range && Q.IsReady() && thp < qdmg)
                    return;
                if (Player.Distance(target.ServerPosition) <= Orbwalking.GetRealAutoAttackRange(Player) &&target.Health < Player.GetAutoAttackDamage(target))
                    return;
                if (Player.Distance(target.Position) <= 600 && target.HasBuff("summonerdot") &&
                    target.Health < IgniteDamage(target))
                    return;

                //When to use R
                if (thp < edmg + rdmg && E.IsReady()
                    || thp < qdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + hexdmg && Q.IsReady() && hextech.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + hexdmg && hextech.IsReady()
                    || thp < edmg + qdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + hexdmg && E.IsReady() && Q.IsReady() && hextech.IsReady()
                    || thp < edmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + hexdmg && E.IsReady() && hextech.IsReady()
                    || thp < edmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + markdmg + qdmg + hexdmg && Q.IsReady() && E.IsReady() && hextech.IsReady()
                    || thp < edmg + markdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + hexdmg && E.IsReady() && hextech.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + markdmg + qdmg + hexdmg && Q.IsReady() && hextech.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + markdmg + hexdmg && target.HasBuff("AkaliMota") && hextech.IsReady()

                    || thp < qdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + bilgedmg && Q.IsReady() && cutlass.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + bilgedmg && cutlass.IsReady()
                    || thp < edmg + qdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + bilgedmg && E.IsReady() && Q.IsReady() && cutlass.IsReady()
                    || thp < edmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + bilgedmg && E.IsReady() && cutlass.IsReady()
                    || thp < edmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + markdmg + qdmg + bilgedmg && Q.IsReady() && E.IsReady() && cutlass.IsReady()
                    || thp < edmg + markdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + bilgedmg && E.IsReady() && cutlass.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + markdmg + qdmg + bilgedmg && Q.IsReady() && cutlass.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + markdmg + bilgedmg && target.HasBuff("AkaliMota") && cutlass.IsReady()

                    || thp < qdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + tiamatdmg && Q.IsReady() && tiamat.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + tiamatdmg && tiamat.IsReady()
                    || thp < edmg + qdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + tiamatdmg && E.IsReady() && Q.IsReady() && tiamat.IsReady()
                    || thp < edmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + tiamatdmg && E.IsReady() && tiamat.IsReady()
                    || thp < edmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + markdmg + qdmg + tiamatdmg && Q.IsReady() && E.IsReady() && tiamat.IsReady()
                    || thp < edmg + markdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + tiamatdmg && E.IsReady() && tiamat.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + markdmg + qdmg + tiamatdmg && tiamat.IsReady() && Q.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + markdmg + tiamatdmg && tiamat.IsReady() && target.HasBuff("AkaliMota")

                    || thp < qdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + hydradmg && Q.IsReady() && hydra.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + hydradmg && hydra.IsReady()
                    || thp < edmg + qdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + hydradmg && E.IsReady() && Q.IsReady() && hydra.IsReady()
                    || thp < edmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + hydradmg && E.IsReady() && hydra.IsReady()
                    || thp < edmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + markdmg + qdmg + hydradmg && Q.IsReady() && E.IsReady() && hydra.IsReady()
                    || thp < edmg + markdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + hydradmg && E.IsReady() && hydra.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + markdmg + qdmg + hydradmg && hydra.IsReady() && Q.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + markdmg + hydradmg && hydra.IsReady() && target.HasBuff("AkaliMota")

                    || thp < qdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) && Q.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) 
                    || thp < edmg + qdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) && E.IsReady() && Q.IsReady()
                    || thp < edmg + rdmg * 2 + Player.GetAutoAttackDamage(target) && E.IsReady()
                    || thp < edmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + markdmg + qdmg && Q.IsReady() && E.IsReady()
                    || thp < edmg + markdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) && E.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target)+ markdmg + qdmg && Q.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + markdmg && target.HasBuff("AkaliMota")

                    || thp < qdmg + rdmg * 2 + IgniteDamage(target) + Player.GetAutoAttackDamage(target) && Q.IsReady() && Ignite.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + IgniteDamage(target) && Ignite.IsReady()
                    || thp < edmg + qdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + IgniteDamage(target) && E.IsReady() && Q.IsReady() && Ignite.IsReady()
                    || thp < edmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + IgniteDamage(target) && E.IsReady() && Ignite.IsReady()
                    || thp < edmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + IgniteDamage(target) + markdmg + qdmg && Q.IsReady() && E.IsReady() && Ignite.IsReady()
                    || thp < edmg + markdmg + rdmg * 2 + Player.GetAutoAttackDamage(target) + IgniteDamage(target) && E.IsReady() && Ignite.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + IgniteDamage(target) + markdmg + qdmg && Q.IsReady() && Ignite.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + IgniteDamage(target) + markdmg && target.HasBuff("AkaliMota") && Ignite.IsReady()

                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + tiamatdmg && tiamat.IsReady() 
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + hydradmg && hydra.IsReady() 
                    || thp < hexdmg && hextech.IsReady()
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + bilgedmg && cutlass.IsReady() 
                    || thp < rdmg * 2 + Player.GetAutoAttackDamage(target) + botrkdmg && botrk.IsReady())
                {
                    //TURRET CHECK FOR ENEMY COUNT
                    if (target.CountEnemiesInRange(1200) >= Config.Item("eslider").GetValue<Slider>().Value &&
                        Config.Item("Rcheck").GetValue<bool>())
                        return;

                    if (ObjectManager.Get<Obj_AI_Turret>()
                            .Any( //TURRET CHECK
                                t => t.Team != Player.Team && !t.IsDead && t.Distance(RendPos, true) < 775*775 &&
                                    Config.Item("rturretcheck").GetValue<bool>()) && Player.HealthPercent <= Config.Item("turrethp").GetValue<Slider>().Value)
                        return;
                    
                        //ACTUAL R ENGAGE IF KILLABLE
                        if (target.CountEnemiesInRange(1200) <= Config.Item("eslider").GetValue<Slider>().Value &&
                            Config.Item("Rcheck").GetValue<bool>())
                        {
                            R.Cast(target, true);
                        }
                        else if (Config.Item("UseR").GetValue<bool>())
                        {
                            R.Cast(target, true);
                        }



                        //GAPCLOSE
                        if (Player.Distance(target.ServerPosition) >= rrange)

                            R.Cast(target, true);

                        else if (Player.Distance(target.ServerPosition) >= rrange)
                            R.Cast(target);
                    
                }
            }
            //IF RKILL DISABLED

            //Turret check for enemy count
            if (target.CountEnemiesInRange(1200) >= Config.Item("eslider").GetValue<Slider>().Value &&
                Config.Item("Rcheck").GetValue<bool>())
                return;

            if (ObjectManager.Get<Obj_AI_Turret>()
                    .Any( //TURRET CHECK
                        t => t.Team != Player.Team && !t.IsDead && t.Distance(RendPos, true) < 775 * 775 &&
                            Config.Item("rturretcheck").GetValue<bool>()) && Player.HealthPercent <= Config.Item("turrethp").GetValue<Slider>().Value)
                            return;

                    if (!Config.Item("Rkill").GetValue<bool>() && Player.Distance(target.ServerPosition) >= rrange 
                    && target.CountEnemiesInRange(1200) <= Config.Item("eslider").GetValue<Slider>().Value &&
                        Config.Item("Rcheck").GetValue<bool>())
                    
                        R.Cast(target, true);

                    else if (!Config.Item("Rkill").GetValue<bool>() && Player.Distance(target.ServerPosition) >= rrange)
                        R.Cast(target);


        }

        private static void Elogic()
        {
            Ignite = Player.GetSpellSlot("summonerdot");
            var target = TargetSelector.GetTarget(R.Range * 2 + 500, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;
            if (E.IsReady() && Player.Distance(target.ServerPosition) <= Orbwalking.GetRealAutoAttackRange(Player) &&
                Config.Item("UseE").GetValue<bool>() && target.IsValidTarget(E.Range))
                E.Cast();

            //DONT USE E CANCEL -> IF KILLABLE
            if (E.IsReady() && target.Health < E.GetDamage(target) && target.Distance(Player.Position) < E.Width)
                E.Cast(Player, true);

            if (E.IsReady() && target.Health < E.GetDamage(target) + IgniteDamage(target) &&
                Config.Item("UseE").GetValue<bool>() && Config.Item("UseIgnite").GetValue<bool>()
                && target.Distance(Player.Position) < E.Width)
            {
                E.Cast(Player, true);
                Player.Spellbook.CastSpell(Ignite, target);
            }
        }

        private static void Item()
        {
            Ignite = Player.GetSpellSlot("summonerdot");
            var target = TargetSelector.GetTarget(R.Range * 2 + 500, TargetSelector.DamageType.Magical);
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
            var target = TargetSelector.GetTarget(R.Range*2 + 500, TargetSelector.DamageType.Magical);
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
                    Render.Circle.DrawCircle(orbwalkert.Position, 10, System.Drawing.Color.DeepSkyBlue, 7);


                //Draw killable minion with Q
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget() && x.IsEnemy
                                                                                     &&
                                                                                     x.Distance(Player.ServerPosition) <
                                                                                     Q.Range + 100 &&
                                                                                     x.Health < Q.GetDamage(x)))
                {
                    Render.Circle.DrawCircle(minion.Position, 50, System.Drawing.Color.DarkRed, 10);
                }
               //Draw Killable minion E
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget() && x.IsEnemy
                                                                                     && x.Distance(Player.ServerPosition) <
                                                                                     E.Range &&
                                                                                     x.Health < E.GetDamage(x)))
                {
                    Render.Circle.DrawCircle(minion.Position, 50, System.Drawing.Color.DarkRed, 10);
                }                

            }
        }
    }

