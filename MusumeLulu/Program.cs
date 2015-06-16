using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;
using Color = System.Drawing.Color;

namespace MusumeLulu
{
    internal class Program
    {
        public const string ChampName = "Lulu";
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;
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

            Notifications.AddNotification("MusumeLulu Loaded!", 1000);

            Q = new Spell(SpellSlot.Q,925);
            W = new Spell(SpellSlot.W,650);
            E = new Spell(SpellSlot.E,650);
            R = new Spell(SpellSlot.R,900);
            Q.SetSkillshot(0.25f, 70, 1450, false, SkillshotType.SkillshotLine);

            Config = new Menu("MusumeLulu", "lulu", true);
            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("Target Selector", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("Combo Settings", "Combo Settings"));
            var harass = Config.AddSubMenu(new Menu("Harass Settings", "Harass Settings"));
            var killsteal = Config.AddSubMenu(new Menu("Killsteal Settings", "Killsteal Settings"));
            var laneclear = Config.AddSubMenu(new Menu("Laneclear Settings", "Laneclear Settings"));
            var lasthit = Config.AddSubMenu(new Menu("Lasthit Settings", "Lasthit Settings"));
            var jungleclear = Config.AddSubMenu(new Menu("Jungle Settings", "Jungle Settings"));
            var misc = Config.AddSubMenu(new Menu("Misc Settings", "Misc Settings"));
            var drawing = Config.AddSubMenu(new Menu("Draw Settings", "Draw Settings"));

            //Advanced Settings //[E] Settings

            //[W] Settings
            combo.SubMenu("[Advanced Features Q]")
                .AddItem(new MenuItem("eq", "Use [E] on Minion to extend [Q] range ").SetValue(false));

            combo.SubMenu("[Advanced Features W]")
                .AddItem(new MenuItem("wgap", "Use [W] on Gapcloser ").SetValue(false));

            combo.SubMenu("[Advanced Features E]").AddItem(new MenuItem("eonhp", "Use [E] on < % HP ").SetValue(false));
            //[R] Settings
            combo.SubMenu("[Advanced Features R]")
                .AddItem(new MenuItem("raoe", "Use R if it knocks up X amount of champions").SetValue(true));


            //ITEMS
            combo.SubMenu("[Item Settings]").AddItem(new MenuItem("UseItems", "Use Offensive Items").SetValue(true));
            //Use dat blue support item thing
            //uhm something something zhonyas

            //Combo Settings
            combo.AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            combo.AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            combo.AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            combo.AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));




            //DRAWING
            drawing.AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            drawing.SubMenu("Spell Drawings")
                .AddItem(new MenuItem("Qdraw", "Draw Q Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.SubMenu("Spell Drawings")
                .AddItem(new MenuItem("Wdraw", "Draw W Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.SubMenu("Spell Drawings")
                .AddItem(new MenuItem("Edraw", "Draw E Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.SubMenu("Spell Drawings")
                .AddItem(new MenuItem("Rdraw", "Draw R Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            //drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("RGdraw", "Draw R Gapclose Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.SubMenu("Spell Drawings")
                .AddItem(new MenuItem("CircleThickness", "Circle Thickness").SetValue(new Slider(7, 30, 0)));

            drawing.SubMenu("Misc Drawings")
                .AddItem(new MenuItem("drawdmg", "Draw Damage on Enemy HPbar").SetValue(true));


            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += OnDraw;


        }

        private static void OnDraw(EventArgs args)
        {
            Render.Circle.DrawCircle(PixPosition(), 20, Color.Blue, 5);
        }

        private static Vector3 PixPosition()
        {
            foreach (var pix in ObjectManager.Get<Obj_AI_Minion>().Where(pix => pix.Name == "RobotBuddy" && pix.IsAlly))
            {
                return pix.Position;
            }
            return ObjectManager.Player.Position;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {


            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Laneclear();
                    break;
            }
        }



        private static void Laneclear()
        {

            var AA = MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                Orbwalking.GetRealAutoAttackRange(Player));

            //Passive damage added for laneclear/lasthit
            foreach (var minion in AA)
            {
                var qcollision = Q.GetCollision(Player.ServerPosition.To2D(), new List<Vector2> { Q.GetPrediction(minion).CastPosition.To2D() });
                var minioncol = qcollision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);
                var pdmg = Player.CalcDamage(minion, Damage.DamageType.Magical,
                    3 + (Player.Level*4) + (Player.FlatMagicDamageMod*0.05));

                if (minion.Health < pdmg && minioncol < 1 && minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
                {
                    Orbwalker.ForceTarget(minion);
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }

                //AddQfarmlineprediction
                //Elasthitprediction (Siegeminions)
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (!target.IsValid && target.IsInvulnerable)
                return;

            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>())
                Qlogic();

            // Remember to get prediction from Minion position and not player position if using E-Q.

           // if (E.IsReady() && Config.Item("UseE").GetValue<bool>())

               // Elogic();
               // E on ally/self if ally gets dmg or ccd
               // E on ally/self on dangerous spells syndra r, zed r and stuff

           // if (W.IsReady() && Config.Item("UseW").GetValue<bool>())

             //   Wlogic();
             //   W on gapcloser, W on enemy to protect allies, W to interrupt spells (will add list for certain spells, kata R, nunu R, weedwick R

           // if (R.IsReady() && Config.Item("UseR").GetValue<bool>())

               // Rlogic();

           // if (Config.Item("UseItems").GetValue<bool>())

               // Itemlogic();
        }

        private static void Qlogic()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Config.Item("UseQ").GetValue<bool>())
                 Q.Cast(target);


            foreach (var minion in
                ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget() && minion.IsEnemy &&
                                                                   minion.Distance(Player.ServerPosition) <= E.Range ||
                                                                   minion.IsAlly &&
                                                                   Player.Distance(minion.Position) <= E.Range))
            {
                if (E.IsReady() && target.Distance(Player.Position) > Q.Range && minion.Distance(target) < Q.Range)
                {
                    E.Cast(minion);
                    Q.Cast(target);
                }
            }
            if (PixPosition().Distance(target.Position) < Q.Range)
            {
                Q.SetSkillshot(0.25f, 70, 1450, false, SkillshotType.SkillshotLine, PixPosition(), PixPosition());   
            }


        }
        public static Obj_AI_Base GetFarthestMinion(Vector3 playerpos, Vector3 enemypos)
        {
            return ObjectManager.Get<Obj_AI_Base>().Where(x => x.Distance(playerpos) < x.Distance(enemypos) && !x.IsInvulnerable && x.IsValidTarget(R.Range)).OrderBy(m => m.Distance(playerpos)).LastOrDefault();

        }
        private static HitChance PredictionQ(string name)
        {
            var qpred = Config.Item(name).GetValue<StringList>();
            switch (qpred.SList[qpred.SelectedIndex])
            {
                case "Low":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High":
                    return HitChance.High;
                case "Very High":
                    return HitChance.VeryHigh;
            }
            return HitChance.VeryHigh;
        }
    }
}



