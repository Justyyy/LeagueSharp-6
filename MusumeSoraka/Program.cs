using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
        public const string ChampName = "Soraka";
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R, Q2;
        public static HpBarIndicator Hpi = new HpBarIndicator();
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        public static GameObject ThreshGameObject;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }


        private static void OnLoad(EventArgs args)
        {
            if (Player.ChampionName != ChampName)
                return;

            Notifications.AddNotification("MusumeSoraka Loaded!", 1000);

            Q = new Spell(SpellSlot.Q, 970);
            W = new Spell(SpellSlot.W, 550);
            E = new Spell(SpellSlot.E, 925);
            R = new Spell(SpellSlot.R);

            //Apply SpellData
            Q.SetSkillshot(0.5f, 300, 1750, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 70f, 1750, false, SkillshotType.SkillshotCircle);

            Config = new Menu("MusumeSoraka", "Soraka", true);
            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("Target Selector", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("Spell Menu", "Spell Menu"));
            var harass = Config.AddSubMenu(new Menu("Harass Settings", "Harass Settings"));
            var misc = Config.AddSubMenu(new Menu("Misc Settings", "Misc Settings"));
            var drawing = Config.AddSubMenu(new Menu("Draw Settings", "Draw Settings"));

            //Advanced Settings //[E] Settings

            //[W] Settings


            foreach (var hero in HeroManager.Allies)
            {

                    combo.SubMenu("[Advanced Features W]")
                        .SubMenu("Whitelist")
                        .AddItem(new MenuItem("allywhitelist." + hero.ChampionName, hero.ChampionName).SetValue(true));
                
            }
            combo.SubMenu("[Advanced Features W]").AddItem(new MenuItem("wonhp", "Use [W] on <= % HP ").SetValue(true));

            foreach (var hero in HeroManager.Allies)
            {
                    combo.SubMenu("[Advanced Features W]")
                        .AddItem(
                            new MenuItem("allyhp." + hero.ChampionName, hero.ChampionName + " Health %").SetValue(
                                new Slider(70, 100, 0)));
                
            }

            combo.SubMenu("[Advanced Features W]").AddItem(new MenuItem("playerhp", "Don't Use W if player HP % <= ").SetValue(new Slider(35, 100, 0)));

            combo.SubMenu("[Advanced Features E]").AddItem(new MenuItem("interrupt", "Use [E] on interruptable spells").SetValue(true));
            combo.SubMenu("[Advanced Features E]").AddItem(new MenuItem("Einterrupt", "Use [E] on immobile targets").SetValue(true));


            harass.AddItem(new MenuItem("harassQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("harassE", "Use E").SetValue(true));
            harass.AddItem(new MenuItem("harassmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));



            foreach (var hero in HeroManager.Allies)
            {
                combo.SubMenu("[Advanced Features R]").SubMenu("Whitelist").AddItem(new MenuItem("allybr." + hero.ChampionName, hero.ChampionName).SetValue(false));
            }
            combo.SubMenu("[Advanced Features R]").AddItem(new MenuItem("ronhp", "Use [R] on <= % HP ").SetValue(false));
            foreach (var hero in HeroManager.Allies)
            {
                combo.SubMenu("[Advanced Features R]").AddItem(new MenuItem("allyr." + hero.ChampionName, hero.ChampionName + " Health %").SetValue(new Slider(20, 100, 0)));
            }



            //ITEMS
            combo.SubMenu("[Item Settings]").AddItem(new MenuItem("UseItems", "Items Usage").SetValue(true));
            //Use dat blue support item thing
            //uhm something something zhonyas

            //Combo Settings
            combo.AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            combo.AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            combo.AddItem(new MenuItem("UseR", "Use R").SetValue(true));

            //DRAWING
            drawing.AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("Qdraw", "Draw Q Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("Wdraw", "Draw W Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("Edraw", "Draw E Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));        
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("CircleThickness", "Circle Thickness").SetValue(new Slider(7, 30, 0)));


            Config.AddItem(new MenuItem("PewPew", "            Prediction Settings"));

            Config.AddItem(new MenuItem("hitchanceQ", "[Q] Hitchance").SetValue(new StringList
                (new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString() }, 3)));
            Config.AddItem(new MenuItem("hitchanceE", "[E] Hitchance").SetValue(new StringList
                (new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString() }, 3)));

            misc.AddItem(new MenuItem("skinhax", "Skin Manager").SetValue(true));
            misc.AddItem(new MenuItem("sorakaskin", "Skin Name").SetValue(new StringList(new[] { "Classic Soraka", "Dryad Soraka", "Divine Soraka", "Celestine Soraka", "Reaper Soraka", "Order of the Banana Soraka" })));

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapCloser_OnEnemyGapcloser;
            GameObject.OnCreate += AntiLeap;
            Obj_AI_Base.OnProcessSpellCast += InterrupterSc;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;

        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (E.IsReady() && sender.IsValidTarget(E.Range) && Config.Item("interrupt").GetValue<bool>())
                E.CastOnUnit(sender);
        }


        private static void InterrupterSc(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.Team != Player.Team)
            {
                    if (args.SData.Name == "KatarinaR" || args.SData.Name == "AlZaharNetherGrasp" ||
                        args.SData.Name == "LucianR" ||
                        args.SData.Name == "CaitlynPiltoverPeacemaker" || args.SData.Name == "RivenMatyr" ||
                        args.SData.Name == "VarusQ" ||
                        args.SData.Name == "AbsoluteZero" || args.SData.Name == "Drain" ||
                        args.SData.Name == "InfiniteDuress" ||
                        args.SData.Name == "MissFortuneBulletTime" || args.SData.Name == "ThreshQ" ||
                        args.SData.Name == "RocketGrabMissile")
                    {
                        if (E.IsReady() && Config.Item("Einterrupt").GetValue<bool>() &&
                            sender.Distance(Player.Position) <= E.Range)
                            E.Cast(sender);                                          
                }
                if (ThreshGameObject.Position.CountEnemiesInRange(250) >= 1)
                    E.Cast(ThreshGameObject.Position);
            }
            foreach (var Object in ObjectManager.Get<Obj_AI_Base>().Where(Obj => Obj.Distance(Player.ServerPosition) < E.Range 
                && Obj.Team != Player.Team && (Obj.HasBuff("teleport_target", true) || Obj.HasBuff("Pantheon_GrandSkyfall_Jump", true))))
            {
                Utility.DelayAction.Add(2500, () => { E.Cast(Object.Position); });
            }
        }


        private static void AntiLeap(GameObject sender, EventArgs args)
        {
            var rengar = HeroManager.Enemies.Find(h => h.ChampionName.Equals("Rengar"));
            //<---- Credits to Asuna (Couldn't figure out how to cast R to Sender so I looked at his vayne ^^
            if (rengar != null)

                if (sender.Name == ("Rengar_LeapSound.troy") && Config.Item("AntiRengar").GetValue<bool>() &&
                    sender.Position.Distance(Player.Position) <= W.Range)
                {
                    E.Cast(rengar);
                }

            var khazix = HeroManager.Enemies.Find(h => h.ChampionName.Equals("Khazix"));

            if (khazix != null)

                if (sender.Name == ("Khazix_Base_E_Tar.troy") && Config.Item("AntiKhazix").GetValue<bool>() &&
                    sender.Position.Distance(Player.Position) <= W.Range)
                {
                    E.Cast(khazix);
                }

            foreach (var enemy in HeroManager.Enemies)
                if (sender.Name.Contains("Thresh_Base_Lantern") && Player.Distance(sender.Position) < E.Range &&
                    sender.IsEnemy)
                {
                    ThreshGameObject = sender;
                }

        }

        private static void AntiGapCloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range) && Config.Item("antigap").GetValue<bool>())
                E.CastOnUnit(gapcloser.Sender);
        }

        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

             if (Config.Item("Qdraw").GetValue<Circle>().Active)
              if (Q.Level > 0)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Config.Item("Qdraw").GetValue<Circle>().Color : Color.Red,
                      Config.Item("CircleThickness").GetValue<Slider>().Value);

            if (Config.Item("Wdraw").GetValue<Circle>().Active)
                if (W.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Config.Item("Wdraw").GetValue<Circle>().Color : Color.Red,
                                                        Config.Item("CircleThickness").GetValue<Slider>().Value);

            if (Config.Item("Edraw").GetValue<Circle>().Active)
                if (E.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range,
                        E.IsReady() ? Config.Item("Edraw").GetValue<Circle>().Color : Color.Red,
                                                        Config.Item("CircleThickness").GetValue<Slider>().Value);

           Render.Circle.DrawCircle(ThreshGameObject.Position, 100, Color.Blue, 300);

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Laneclear();
                    break;
            }


                foreach (var hero in HeroManager.Allies)
                {
                    if (hero.Position.CountEnemiesInRange(800) >= 1 &&
                        Config.Item("allyr." + hero.ChampionName).GetValue<Slider>().Value >= hero.HealthPercent
                        && Config.Item("allybr." + hero.ChampionName).GetValue<bool>() &&
                        Config.Item("ronhp").GetValue<bool>() && !hero.IsDead && R.IsReady())
                    {
                        R.Cast(hero);
                    }
                }

                foreach (var hero in HeroManager.Allies)
                {
                    if (!hero.IsDead && hero.Distance(Player.Position) < W.Range && hero.HealthPercent  <= Config.Item("allyhp." + hero.ChampionName).GetValue<Slider>().Value &&
                        Config.Item("wonhp").GetValue<bool>() &&
                        Config.Item("allywhitelist." + hero.ChampionName).GetValue<bool>() && Player.HealthPercent >= Config.Item("playerhp").GetValue<Slider>().Value && !hero.InFountain())
                    {
                        W.Cast(hero);
                    }
                }

                //SkinChanger
                Player.SetSkin(Player.BaseSkinName, Config.Item("skinhax").GetValue<bool>()
                    ? Config.Item("sorakaskin").GetValue<StringList>().SelectedIndex
                    : Player.BaseSkinId);

            
        }



        private static void Laneclear()
        {

            
        }
        private static void Harass()
        {
            var harassmana = Config.Item("harassmana").GetValue<Slider>().Value;
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && Q.GetPrediction(target).Hitchance >= PredictionQ("hitchanceQ") && Player.ManaPercent >= harassmana)
                Q.Cast(target);

            if (E.IsReady() && Config.Item("UseE").GetValue<bool>() && E.GetPrediction(target).Hitchance >= PredictionE("hitchanceE") && Player.ManaPercent >= harassmana)
                E.Cast(target);
        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (!target.IsValid && target.IsInvulnerable)
                return;

            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && Q.GetPrediction(target).Hitchance >= PredictionQ("hitchanceQ"))
                Q.Cast(target);

            if (E.IsReady() && Config.Item("UseE").GetValue<bool>() && E.GetPrediction(target).Hitchance >= PredictionE("hitchanceE"))
                E.Cast(target);

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
        private static HitChance PredictionE(string name)
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



