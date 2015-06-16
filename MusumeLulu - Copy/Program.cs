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
        public static Spell Q, W, E, R, Q2;
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
            Q2 = new Spell(SpellSlot.Q, 925);
            W = new Spell(SpellSlot.W,650);
            E = new Spell(SpellSlot.E,650);
            R = new Spell(SpellSlot.R,900);

            Q2.SetSkillshot(0.25f, 70, 1450, false, SkillshotType.SkillshotLine, PixPosition(), PixPosition());  
            Q.SetSkillshot(0.25f, 70, 1450, false, SkillshotType.SkillshotLine);

            Config = new Menu("MusumeLulu", "lulu", true);
            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("Target Selector", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("Spell Menu", "Spell Menu"));
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
                .AddItem(new MenuItem("eq", "Use [E] on Minion to extend [Q] range ").SetValue(true));


            foreach (var hero in HeroManager.Allies)
            {
                combo.SubMenu("[Advanced Features E]").SubMenu("Blacklist").AddItem(new MenuItem("allybe." + hero.ChampionName, hero.ChampionName).SetValue(false));
            }
            combo.SubMenu("[Advanced Features E]").AddItem(new MenuItem("eonhp", "Use [E] on < % HP ").SetValue(false));

            foreach (var hero in HeroManager.Allies)
            {
                combo.SubMenu("[Advanced Features E]").AddItem(new MenuItem("allye." + hero.ChampionName, hero.ChampionName + " Health %").SetValue(new Slider(80, 100, 0)));
            }

            combo.SubMenu("[Advanced Features E]").AddItem(new MenuItem("eondmg", "Use E on incoming DMG").SetValue(false));

            combo.SubMenu("[Advanced Features W]")
                .AddItem(new MenuItem("Winterrupt", "Use [W] on interruptable spells").SetValue(false));

            //[R] Settings
            combo.SubMenu("[Advanced Features R]")
                .AddItem(new MenuItem("raoe", "Use R if it knocks up X amount of champions").SetValue(true));
            combo.SubMenu("[Advanced Features R]")
                .AddItem(new MenuItem("Rinterrupt", "Use [R] on interruptable spells").SetValue(true));

            foreach (var hero in HeroManager.Allies)
            {
                combo.SubMenu("[Advanced Features R]").SubMenu("Blacklist").AddItem(new MenuItem("allybr." + hero.ChampionName, hero.ChampionName).SetValue(false));
            }
            combo.SubMenu("[Advanced Features R]").AddItem(new MenuItem("ronhp", "Use [R] on < % HP ").SetValue(false));
            foreach (var hero in HeroManager.Allies)
            {
                combo.SubMenu("[Advanced Features R]").AddItem(new MenuItem("allyr." + hero.ChampionName, hero.ChampionName + " Health %").SetValue(new Slider(80, 100, 0)));
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
            combo.AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));




            //DRAWING
            drawing.AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("Qdraw", "Draw Q Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("Wdraw", "Draw W Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("Edraw", "Draw E Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("Rdraw", "Draw R Range").SetValue(new Circle(true, System.Drawing.Color.IndianRed)));           
            drawing.SubMenu("Spell Drawings").AddItem(new MenuItem("CircleThickness", "Circle Thickness").SetValue(new Slider(7, 30, 0)));

            drawing.SubMenu("Misc Drawings").AddItem(new MenuItem("dmgdrawer", "[Damage Indicator]:", true).SetValue
                (new StringList(new[] { "Custom", "Common" })));

            Config.AddItem(new MenuItem("PewPew", "            Prediction Settings"));

            Config.AddItem(new MenuItem("hitchanceQ", "[Q] Hitchance").SetValue(new StringList
                (new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString() }, 3)));

            misc.AddItem(new MenuItem("skinhax", "Skin Manager").SetValue(true));
            misc.AddItem(new MenuItem("luluskin", "Skin Name").SetValue(new StringList(new[] { "Classic Lulu", "Bittersweet Lulu", "Wicked Lulu", "Dragon Trainer Lulu", "Winter Wonder Lulu" })));

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapCloser_OnEnemyGapcloser;
            GameObject.OnCreate += AntiLeap;
            Obj_AI_Base.OnProcessSpellCast += InterrupterSc;

        }


        private static void hpshield()
        {
            //E on Health percentage
            if (E.IsReady() && Player.Mana >= E.Instance.ManaCost || R.IsReady() && Player.Mana >= R.Instance.ManaCost)
            {
                foreach (var hero in HeroManager.Allies)
                {
                    if (Config.Item("allybe." + hero.ChampionName).GetValue<bool>() && Config.Item("eonhp").GetValue<bool>() || Config.Item("allybr." + hero.ChampionName).GetValue<bool>() && Config.Item("ronhp").GetValue<bool>())
                    {
                            if (hero.Position.CountEnemiesInRange(800) >= 1 && Config.Item("allye." + hero.ChampionName).GetValue<Slider>().Value >= hero.HealthPercent)
                                E.Cast(hero);

                        if (Config.Item("allyr." + hero.ChampionName).GetValue<Slider>().Value >= hero.HealthPercent &&
                            hero.Position.CountEnemiesInRange(800) >= 1)
                            R.Cast(hero);
                    
                    }
                }
            }
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
                        if (W.IsReady() && Config.Item("Winterrupt").GetValue<bool>() &&
                            sender.Distance(Player.Position) <= W.Range)
                            W.Cast(sender);

                        if (sender.Distance(Player.Position) <= W.Range && W.IsReady() && !R.IsReady())
                            return;

                        if (args.SData.Name == "KatarinaR" || args.SData.Name == "AlZaharNetherGrasp" ||
                            args.SData.Name == "AbsoluteZero" ||
                            args.SData.Name == "InfiniteDuress" ||
                            args.SData.Name == "MissFortuneBulletTime")
                        {
                            foreach 
                                (var hero in
                                    ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && h.IsValidTarget(R.Range)))
                                if (R.IsReady() && Config.Item("Rinterrupt").GetValue<bool>() &&
                                    sender.Distance(hero.Position) <= 150)
                                    R.Cast(hero);
                        
                    }

                }
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
                    W.Cast(rengar);
                    E.Cast(rengar);
                }

            var khazix = HeroManager.Enemies.Find(h => h.ChampionName.Equals("Khazix"));

            if (khazix != null)

                if (sender.Name == ("Khazix_Base_E_Tar.troy") && Config.Item("AntiKhazix").GetValue<bool>() &&
                    sender.Position.Distance(Player.Position) <= W.Range)
                {
                    W.Cast(khazix);
                    E.Cast(khazix);
                }
        }

        private static void AntiGapCloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (W.IsReady() && gapcloser.Sender.IsValidTarget(W.Range) && Config.Item("antigap").GetValue<bool>())
                W.CastOnUnit(gapcloser.Sender);
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

            hpshield();







          //SkinChanger
          Player.SetSkin(Player.BaseSkinName, Config.Item("skinhax").GetValue<bool>() 
          ? Config.Item("luluskin").GetValue<StringList>().SelectedIndex : Player.BaseSkinId);

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

                if (minion.Health < pdmg + Player.GetAutoAttackDamage(minion) && minioncol < 1 && minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
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


           // if (E.IsReady() && Config.Item("UseE").GetValue<bool>())

               // Elogic();
               // E on ally/self if ally gets dmg or ccd
               // E on ally/self on dangerous spells syndra r, zed r and stuff

           // if (W.IsReady() && Config.Item("UseW").GetValue<bool>())

             //   Wlogic();
             //   W on gapcloser, W on enemy to protect allies, W to interrupt spells (will add list for certain spells, kata R, nunu R, weedwick R

             if (R.IsReady() && Config.Item("UseR").GetValue<bool>())

               Rlogic();

           // if (Config.Item("UseItems").GetValue<bool>())

               // Itemlogic();
        }

        private static void Qlogic()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var ptarget = TargetSelector.GetTarget(1800, TargetSelector.DamageType.Magical);



              foreach (var enemy in HeroManager.Enemies)
                if (E.IsReady() && target.Distance(Player.Position) > Q.Range && GetFarthestMinion(Player.Position, enemy.Position).Distance(Player.Position) <= E.Range
                    && GetFarthestMinion(Player.Position, enemy.Position).Distance(enemy.Position) < Q2.Range )
                {  
                    E.Cast(GetFarthestMinion(Player.Position, enemy.Position));
                    Q2.Cast(enemy);
                }

              foreach (var enemy in HeroManager.Enemies)
            if (Q.IsReady() && Q2.GetPrediction(enemy).Hitchance >= PredictionQ("hitchanceQ"))
                Q2.Cast(enemy);

            

            if (Config.Item("UseQ").GetValue<bool>() && Q.GetPrediction(target).Hitchance >= PredictionQ("hitchanceQ"))
                Q.Cast(target);

        }
        public static Obj_AI_Base GetFarthestMinion(Vector3 playerpos, Vector3 enemypos)
        {
            return ObjectManager.Get<Obj_AI_Base>().Where(x => x.Distance(playerpos) < x.Distance(enemypos) && !x.IsInvulnerable && x.IsValidTarget(R.Range)).OrderBy(m => m.Distance(playerpos)).LastOrDefault();

        }

        private static void Rlogic()
        {
            if (R.IsReady() && Config.Item("UseR").GetValue<bool>())
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && h.IsValidTarget(R.Range)))
                {
                    if (hero.Position.CountEnemiesInRange(150) >= Config.Item("emcslider").GetValue<Slider>().Value &&
                        Config.Item("UseEMC").GetValue<bool>())
                        R.Cast(hero);
                }

            }
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



