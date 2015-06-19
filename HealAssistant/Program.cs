using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;

namespace HealAssistant
{
    internal class Program
    {
        public const string ChampName1 = "Soraka";
        public const string ChampName2 = "Alistar";
        public const string ChampName3 = "Sona";
        public const string ChampName4 = "Taric";
        public const string ChampName5 = "Nidalee";
        public const string ChampName6 = "Nami";
        public const string ChampName7 = "Kayle";
        public static Menu Config;
        private static bool IsCougar;
        public static Spell Q, W, E, R;
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }


        private static void OnLoad(EventArgs args)
        {

            Notifications.AddNotification("Heal Assistant Loaded!", 1000);
            Config = new Menu("Heal Assistant", "HA", true);

            //Soraka
            if (Player.ChampionName == ChampName1)
            {
                W = new Spell(SpellSlot.W, 550);
                R = new Spell(SpellSlot.R);

                Config.SubMenu("Soraka [W] Settings").AddItem(new MenuItem("onhp", "Use [W] on <= % HP ").SetValue(true));

                foreach (var hero in HeroManager.Allies)
                {
                Config.SubMenu("Soraka [W] Settings").SubMenu("Whitelist")
                        .AddItem(new MenuItem("allywhitelist." + hero.ChampionName, hero.ChampionName).SetValue(true));

                }

                foreach (var hero in HeroManager.Allies)
                {
                    Config.SubMenu("Soraka [W] Settings").AddItem(
                            new MenuItem("allyhp." + hero.ChampionName, hero.ChampionName + " Health %").SetValue(
                                new Slider(65, 100, 0)));

                }
                Config.SubMenu("Soraka [W] Settings")
                     .AddItem(new MenuItem("playerhp", "Don't Use W if player HP % <= ").SetValue(new Slider(35, 100, 0)));

                Config.SubMenu("Soraka [R] Settings").AddItem(new MenuItem("ronhp", "Use [R] on <= % HP ").SetValue(true));
                foreach (var hero in HeroManager.Allies)
                {
                    Config.SubMenu("Soraka [R] Settings")
                        .SubMenu("Whitelist")
                        .AddItem(new MenuItem("allybr." + hero.ChampionName, hero.ChampionName).SetValue(true));
                }
                foreach (var hero in HeroManager.Allies)
                {
                    Config.SubMenu("Soraka [R] Settings")
                        .AddItem(
                            new MenuItem("allyr." + hero.ChampionName, hero.ChampionName + " Health %").SetValue(
                                new Slider(20, 100, 0)));
                }

                Config.SubMenu("Soraka [W] Settings").AddItem(new MenuItem("priority", "Heal Priority").SetValue(
                new StringList(new[] { "Most AD", "Most AP", "Lowest HP", "Closest" }, 3)));
            }
            //Alistar
            if (Player.ChampionName == ChampName2)
            {
                E = new Spell(SpellSlot.E, 575);
                Config.SubMenu("Alistar [E] Settings").AddItem(new MenuItem("onhp", "Use [E] on <= % HP ").SetValue(true));
                foreach (var hero in HeroManager.Allies)
                {

                    Config.SubMenu("Alistar [E] Settings").SubMenu("Whitelist")
                            .AddItem(new MenuItem("allywhitelist." + hero.ChampionName, hero.ChampionName).SetValue(true));

                }

                foreach (var hero in HeroManager.Allies)
                {
                    Config.SubMenu("Alistar [E] Settings").AddItem(
                            new MenuItem("allyhp." + hero.ChampionName, hero.ChampionName + " Health %").SetValue(
                                new Slider(65, 100, 0)));

                }
                Config.SubMenu("Alistar [E] Settings").AddItem(new MenuItem("manaz", "Don't use [E] if Mana below is %").SetValue(false));
                Config.SubMenu("Alistar [E] Settings").AddItem(new MenuItem("mana", "Mana %").SetValue(new Slider(35, 100, 0)));

            }
            //SONA
            if (Player.ChampionName == ChampName3)
            {
                W = new Spell(SpellSlot.W, 1000);
                Config.SubMenu("Sona [W] Settings").AddItem(new MenuItem("onhp", "Use [W] on <= % HP ").SetValue(true));

                foreach (var hero in HeroManager.Allies)
                {

                    Config.SubMenu("Sona [W] Settings").SubMenu("Whitelist")
                            .AddItem(new MenuItem("allywhitelist." + hero.ChampionName, hero.ChampionName).SetValue(true));

                }

                foreach (var hero in HeroManager.Allies)
                {
                    Config.SubMenu("Sona [W] Settings").AddItem(
                        new MenuItem("allyhp." + hero.ChampionName, hero.ChampionName + " Health %").SetValue(
                            new Slider(65, 100, 0)));
                }
                Config.SubMenu("Sona [W] Settings").AddItem(new MenuItem("manaz", "Don't use [W] if Mana below is %").SetValue(false));
                Config.SubMenu("Sona [W] Settings").AddItem(new MenuItem("mana", "Mana %").SetValue(new Slider(35, 100, 0)));

            }
            if (Player.ChampionName == ChampName4)
            {
                Q = new Spell(SpellSlot.Q, 750);
                Config.SubMenu("Taric [Q] Settings").AddItem(new MenuItem("onhp", "Use [Q] on <= % HP ").SetValue(true));

                foreach (var hero in HeroManager.Allies)
                {

                    Config.SubMenu("Taric [Q] Settings").SubMenu("Whitelist")
                            .AddItem(new MenuItem("allywhitelist." + hero.ChampionName, hero.ChampionName).SetValue(true));

                }

                foreach (var hero in HeroManager.Allies)
                {
                    Config.SubMenu("Taric [Q] Settings").AddItem(
                        new MenuItem("allyhp." + hero.ChampionName, hero.ChampionName + " Health %").SetValue(
                            new Slider(65, 100, 0)));
                }
                Config.SubMenu("Taric [Q] Settings").AddItem(new MenuItem("manaz", "Don't use [Q] if Mana below is %").SetValue(false));
                Config.SubMenu("Taric [Q] Settings").AddItem(new MenuItem("mana", "Mana %").SetValue(new Slider(35, 100, 0)));
                Config.SubMenu("Taric [Q] Settings").AddItem(new MenuItem("priority", "Heal Priority").SetValue(
                new StringList(new[] { "Most AD", "Most AP", "Lowest HP", "Closest" }, 3)));

            }
            if (Player.ChampionName == ChampName5)
            {
                E = new Spell(SpellSlot.E, 600);
                Config.SubMenu("Nidalee [E] Settings").AddItem(new MenuItem("onhp", "Use [E] on <= % HP ").SetValue(true));

                foreach (var hero in HeroManager.Allies)
                {

                    Config.SubMenu("Nidalee [E] Settings").SubMenu("Whitelist")
                            .AddItem(new MenuItem("allywhitelist." + hero.ChampionName, hero.ChampionName).SetValue(true));

                }

                foreach (var hero in HeroManager.Allies)
                {
                    Config.SubMenu("Nidalee [E] Settings").AddItem(
                        new MenuItem("allyhp." + hero.ChampionName, hero.ChampionName + " Health %").SetValue(
                            new Slider(65, 100, 0)));
                }
                Config.SubMenu("Nidalee [E] Settings").AddItem(new MenuItem("manaz", "Don't use [E] if Mana below is %").SetValue(false));
                Config.SubMenu("Nidalee [E] Settings").AddItem(new MenuItem("mana", "Mana %").SetValue(new Slider(35, 100, 0)));
                Config.SubMenu("Nidalee [E] Settings").AddItem(new MenuItem("priority", "Heal Priority").SetValue(
                new StringList(new[] { "Most AD", "Most AP", "Lowest HP", "Closest" }, 3)));
            }
            if (Player.ChampionName == ChampName6)
            {
                W = new Spell(SpellSlot.W, 725);
                Config.SubMenu("Nami [W] Settings").AddItem(new MenuItem("onhp", "Use [W] on <= % HP ").SetValue(true));

                foreach (var hero in HeroManager.Allies)
                {

                    Config.SubMenu("Nami [W] Settings").SubMenu("Whitelist")
                            .AddItem(new MenuItem("allywhitelist." + hero.ChampionName, hero.ChampionName).SetValue(true));
                }

                foreach (var hero in HeroManager.Allies)
                {
                    Config.SubMenu("Nami [W] Settings").AddItem(
                        new MenuItem("allyhp." + hero.ChampionName, hero.ChampionName + " Health %").SetValue(
                            new Slider(65, 100, 0)));
                }
                Config.SubMenu("Nami [W] Settings").AddItem(new MenuItem("manaz", "Don't use [W] if Mana below is %").SetValue(false));
                Config.SubMenu("Nami [W] Settings").AddItem(new MenuItem("mana", "Mana %").SetValue(new Slider(35, 100, 0)));
                Config.SubMenu("Nami [W] Settings").AddItem(new MenuItem("priority", "Heal Priority").SetValue(
                new StringList(new[] { "Most AD", "Most AP", "Lowest HP", "Closest" }, 3)));
            }
            if (Player.ChampionName == ChampName7)
            {
                W = new Spell(SpellSlot.W, 900);
                Config.SubMenu("Kayle [W] Settings").AddItem(new MenuItem("onhp", "Use [W] on <= % HP ").SetValue(true));

                foreach (var hero in HeroManager.Allies)
                {

                    Config.SubMenu("Kayle [W] Settings").SubMenu("Whitelist")
                            .AddItem(new MenuItem("allywhitelist." + hero.ChampionName, hero.ChampionName).SetValue(true));
                }

                foreach (var hero in HeroManager.Allies)
                {
                    Config.SubMenu("Kayle [W] Settings").AddItem(
                        new MenuItem("allyhp." + hero.ChampionName, hero.ChampionName + " Health %").SetValue(
                            new Slider(65, 100, 0)));
                }
                Config.SubMenu("Kayle [W] Settings").AddItem(new MenuItem("manaz", "Don't use [W] if Mana below is %").SetValue(false));
                Config.SubMenu("Kayle [W] Settings").AddItem(new MenuItem("mana", "Mana %").SetValue(new Slider(35, 100, 0)));
                Config.SubMenu("Kayle [W] Settings").AddItem(new MenuItem("priority", "Heal Priority").SetValue(
                new StringList(new[] { "Most AD", "Most AP", "Lowest HP", "Closest" }, 3)));
            }

            //Menu for all Champions:

            var mikael = Config.AddSubMenu(new Menu("Mikael's Crucible", "Mikael's Crucible"));
            foreach (var hero in HeroManager.Allies)
                mikael.SubMenu("Whitelist")
                 .AddItem(new MenuItem("mikael." + hero.ChampionName, hero.ChampionName).SetValue(true));

            var mikaelz = mikael.AddSubMenu(new Menu("CC List", "CC List"));
            var heal = Config.AddSubMenu(new Menu("Summoner Heal", "Summoner Heal"));
            heal.AddItem(new MenuItem("Soon", "Currently not Supported").SetValue(true));

            var draw = Config.AddSubMenu(new Menu("Draw Settings", "Draw Settings"));
            draw.AddItem(new MenuItem("drawhp", "Draw Ally HP%").SetValue(true));
           
            var mikaelz1 = mikael.AddSubMenu(new Menu("Special Debuffs", "Special Debuffs"));
            mikaelz1.AddItem(new MenuItem("exh", "Exhaust").SetValue(true));
            mikael.AddItem(new MenuItem("UseMik", "[Use Mikael's Crucible on CC/Debuffs]").SetValue(true));

            mikaelz.AddItem(new MenuItem("stuns", "Stuns").SetValue(true));
            mikaelz.AddItem(new MenuItem("charms", "Charms").SetValue(true));
            mikaelz.AddItem(new MenuItem("taunts", "Taunts").SetValue(true));
            mikaelz.AddItem(new MenuItem("fears", "Fears").SetValue(true));
            mikaelz.AddItem(new MenuItem("snares", "Snares").SetValue(true));
            mikaelz.AddItem(new MenuItem("slows", "Slows").SetValue(false));

            Config.AddToMainMenu();

            Game.OnUpdate += HealManager;
            Drawing.OnDraw += healdraw;

        }

        private static void healdraw(EventArgs args)
        {
            foreach (var hero in HeroManager.Allies)
            {
                var pos = hero.HPBarPosition;
                if (!hero.IsDead && !hero.IsMe && Config.Item("drawhp").GetValue<bool>())
                    Drawing.DrawText(pos.X + 40, pos.Y - 25, Color.LawnGreen, hero.HealthPercent.ToString("#.#") + "% HP");
              }
            
        }

        private static void Mikael()
        {
            var mikael = ItemData.Mikaels_Crucible.GetItem();

            foreach (var hero in HeroManager.Allies)
            {
                if (Config.Item("mikael." + hero.ChampionName).GetValue<bool>() && hero.IsValidTarget(mikael.Range))
                {
                    if (hero.HasBuffOfType(BuffType.Stun) && Config.Item("stuns").GetValue<bool>() ||
                        hero.HasBuffOfType(BuffType.Charm) && Config.Item("charms").GetValue<bool>() ||
                        hero.HasBuffOfType(BuffType.Fear) && Config.Item("fears").GetValue<bool>() ||
                        hero.HasBuffOfType(BuffType.Snare) && Config.Item("snares").GetValue<bool>() ||
                        hero.HasBuffOfType(BuffType.Taunt) && Config.Item("taunts").GetValue<bool>() ||
                        hero.HasBuffOfType(BuffType.Slow) && Config.Item("slows").GetValue<bool>() || 
                        hero.HasBuffOfType(BuffType.CombatDehancer) && Config.Item("exh").GetValue<bool>())

                        mikael.Cast(hero);
                }
            }

        }
        private static void CheckSpells()
        {
            if (Player.Spellbook.GetSpell(SpellSlot.Q).Name == "JavelinToss" ||
                Player.Spellbook.GetSpell(SpellSlot.W).Name == "Bushwhack" ||
                Player.Spellbook.GetSpell(SpellSlot.E).Name == "PrimalSurge")
            {
                IsCougar = false;
            }
            if (Player.Spellbook.GetSpell(SpellSlot.Q).Name == "Takedown" ||
                Player.Spellbook.GetSpell(SpellSlot.W).Name == "Pounce" ||
                Player.Spellbook.GetSpell(SpellSlot.E).Name == "Swipe")
            {
                IsCougar = true;
            }
        }

        private static void HealManager(EventArgs args)
        {
            CheckSpells();

            if (Config.Item("UseMik").GetValue<bool>())
                Mikael();

            if (Player.ChampionName == ChampName1) //Soraka
            {
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


                if (!GetHealTargetSor().IsDead && GetHealTargetSor().Distance(Player.Position) < W.Range &&
                    GetHealTargetSor().HealthPercent <=
                    Config.Item("allyhp." + GetHealTargetSor().ChampionName).GetValue<Slider>().Value &&
                    Config.Item("onhp").GetValue<bool>() &&
                    Config.Item("allywhitelist." + GetHealTargetSor().ChampionName).GetValue<bool>() &&
                    Player.HealthPercent >= Config.Item("playerhp").GetValue<Slider>().Value &&
                    !GetHealTargetSor().InFountain())
                {
                    W.Cast(GetHealTargetSor());
                }   
            }
            if (Player.ManaPercent <= Config.Item("mana").GetValue<Slider>().Value &&
            Config.Item("manaz").GetValue<bool>())
                return;

            if (Player.ChampionName == ChampName2) //Alistar
            {
                foreach (var hero in HeroManager.Allies)
                if (!hero.IsDead && hero.Distance(Player.Position) < E.Range &&
                    hero.HealthPercent <=
                    Config.Item("allyhp." + hero.ChampionName).GetValue<Slider>().Value &&
                    Config.Item("onhp").GetValue<bool>() &&
                    Config.Item("allywhitelist." + hero.ChampionName).GetValue<bool>() &&
                    !hero.InFountain())
                {
                    E.Cast();
                }
            }
            if (Player.ChampionName == ChampName3) //Sona
            {
                foreach (var hero in HeroManager.Allies)
                if (!hero.IsDead && hero.Distance(Player.Position) < W.Range &&
                    hero.HealthPercent <=
                    Config.Item("allyhp." + hero.ChampionName).GetValue<Slider>().Value &&
                    Config.Item("onhp").GetValue<bool>() &&
                    Config.Item("allywhitelist." + hero.ChampionName).GetValue<bool>() &&
                    !hero.InFountain())
                {
                    W.Cast();
                }
            }
            if (Player.ChampionName == ChampName4) //Taric
            {
                if (!GetHealTargetTar().IsDead && GetHealTargetTar().Distance(Player.Position) < Q.Range &&
                    GetHealTargetTar().HealthPercent <=
                    Config.Item("allyhp." + GetHealTargetTar().ChampionName).GetValue<Slider>().Value &&
                    Config.Item("onhp").GetValue<bool>() &&
                    Config.Item("allywhitelist." + GetHealTargetTar().ChampionName).GetValue<bool>() &&
                    !GetHealTargetTar().InFountain())
                {
                    Q.Cast(GetHealTargetTar());
                }
            }
            if (Player.ChampionName == ChampName6) //Nami
            {
                if (!GetHealTargetNam().IsDead && GetHealTargetNam().Distance(Player.Position) < W.Range &&
                    GetHealTargetNam().HealthPercent <=
                    Config.Item("allyhp." + GetHealTargetNam().ChampionName).GetValue<Slider>().Value &&
                    Config.Item("onhp").GetValue<bool>() &&
                    Config.Item("allywhitelist." + GetHealTargetNam().ChampionName).GetValue<bool>() &&
                    !GetHealTargetNam().InFountain())
                {
                    W.Cast(GetHealTargetNam());
                }
            }
            if (Player.ChampionName == ChampName7) //Kayle
            {
                if (!GetHealTargetNam().IsDead && GetHealTargetNam().Distance(Player.Position) < W.Range &&
                    GetHealTargetNam().HealthPercent <=
                    Config.Item("allyhp." + GetHealTargetNam().ChampionName).GetValue<Slider>().Value &&
                    Config.Item("onhp").GetValue<bool>() &&
                    Config.Item("allywhitelist." + GetHealTargetNam().ChampionName).GetValue<bool>() &&
                    !GetHealTargetNam().InFountain())
                {
                    W.Cast(GetHealTargetNam());
                }
            }
            if (Player.ChampionName == ChampName5 && !IsCougar) //Nidalee
            {
                if (!GetHealTargetNid().IsDead && GetHealTargetNid().Distance(Player.Position) < E.Range &&
                    GetHealTargetNid().HealthPercent <=
                    Config.Item("allyhp." + GetHealTargetNid().ChampionName).GetValue<Slider>().Value &&
                    Config.Item("onhp").GetValue<bool>() &&
                    Config.Item("allywhitelist." + GetHealTargetNid().ChampionName).GetValue<bool>() &&
                    !GetHealTargetNid().InFountain())
                {
                    E.Cast(GetHealTargetNid());
                }
            }

        }

        private static Obj_AI_Hero GetHealTargetNid()
        {
            switch (Config.Item("priority").GetValue<StringList>().SelectedIndex)
            {
                case 0: // MostAD
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(E.Range, false) && ally.IsDead && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderByDescending(dmg => dmg.TotalAttackDamage())
                            .First();
                case 1: // MostAP
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(E.Range, false) && !ally.IsDead && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderByDescending(ap => ap.TotalMagicalDamage())
                            .First();

                case 2: //LowestHP
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(E.Range, false) && !ally.IsDead && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderBy(health => health.HealthPercent)
                            .First();
                case 3: //Closest - ScienceARK please add
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(E.Range, false) && !ally.IsDead && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderBy(a => a.Distance(Player.Position)).FirstOrDefault();

            }
            return null;
        }
        private static Obj_AI_Hero GetHealTargetSor()
        {
            switch (Config.Item("priority").GetValue<StringList>().SelectedIndex)
            {
                case 0: // MostAD
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(W.Range, false) && ally.IsDead && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && !ally.IsMe && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderByDescending(dmg => dmg.TotalAttackDamage())
                            .First();
                case 1: // MostAP
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(W.Range, false) && !ally.IsDead && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && !ally.IsMe && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderByDescending(ap => ap.TotalMagicalDamage())
                            .First();

                case 2: //LowestHP
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(W.Range, false) && !ally.IsDead && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && !ally.IsMe && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderBy(health => health.HealthPercent)
                            .First();
                case 3: //Closest - ScienceARK please add
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(W.Range, false) && !ally.IsDead && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && !ally.IsMe && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderBy(a => a.Distance(Player.Position)).FirstOrDefault();

            }
            return null;
        }
        private static Obj_AI_Hero GetHealTargetTar()
        {
            switch (Config.Item("priority").GetValue<StringList>().SelectedIndex)
            {
                case 0: // MostAD
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(Q.Range, false) && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && ally.IsDead && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderByDescending(dmg => dmg.TotalAttackDamage())
                            .First();
                case 1: // MostAP
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(Q.Range, false) && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && !ally.IsDead && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderByDescending(ap => ap.TotalMagicalDamage())
                            .First();

                case 2: //LowestHP
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(Q.Range, false) && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && !ally.IsDead && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderBy(health => health.HealthPercent)
                            .First();
                case 3: //Closest - ScienceARK please add
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(Q.Range, false) && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && !ally.IsDead && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderBy(closest => closest.Distance(Player.ServerPosition))
                            .FirstOrDefault();

            }
            return null;
        }
        private static Obj_AI_Hero GetHealTargetNam()
        {
            switch (Config.Item("priority").GetValue<StringList>().SelectedIndex)
            {
                case 0: // MostAD
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(W.Range, false) && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && ally.IsDead && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderByDescending(dmg => dmg.TotalAttackDamage())
                            .First();
                case 1: // MostAP
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(W.Range, false) && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && !ally.IsDead && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderByDescending(ap => ap.TotalMagicalDamage())
                            .First();

                case 2: //LowestHP
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(W.Range, false) && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && !ally.IsDead && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderBy(health => health.HealthPercent)
                            .First();
                case 3: //Closest - ScienceARK please add
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(W.Range, false) && ally.HealthPercent <=
                    Config.Item("allyhp." + ally.ChampionName).GetValue<Slider>().Value && !ally.IsDead && Config.Item("allywhitelist." + ally.ChampionName).GetValue<bool>())
                            .OrderBy(closest => closest.Distance(Player.ServerPosition))
                            .FirstOrDefault();

            }
            return null;
        }
    }
}


