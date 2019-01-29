import os
import unittest
from appium import webdriver
from altunityrunner import AltrunUnityDriver
import xmlrunner
import time
from base import CZBTests

import sys
reload(sys)
sys.setdefaultencoding('utf8')


class CZBTutorialTests(CZBTests):

    def setUp(self):
        super(CZBTutorialTests, self).setUp()
        # self.pass_authentification()

    # def test_tutorial(self):
    #     self.altdriver.wait_for_element('Button_Play').mobile_tap()
    #     time.sleep(1)
    #     self.go_though_all_steps(1,37)

    #     self.driver.save_screenshot('./screenshots/Continue_Button_Tutorial1.png')
    #     self.altdriver.wait_for_element('Button_Continue').mobile_tap()

    # def test_second_tutorial(self):
    #     self.altdriver.wait_for_element('Button_Play').mobile_tap()
    #     self.skip_tutorial()
    #     self.altdriver.wait_for_element("NPC")
    #     time.sleep(1)

    #     self.go_though_all_steps(2,33)

    #     self.driver.save_screenshot('./screenshots/Continue_Button_Tutorial2.png')
    #     self.altdriver.wait_for_element('Button_Continue').mobile_tap()

    def test_tutorial_without_waiting_audio(self):
        # self.altdriver.wait_for_element('Button_Play').mobile_tap()
        time.sleep(5)
        self.go_though_all_steps_without_waiting_for_audio(1, 37)

        self.driver.save_screenshot(
            './screenshots/Continue_Button_Tutorial1.png')
        self.altdriver.wait_for_element('Button_Continue').mobile_tap()

    # def test_second_tutorial_without_waiting_audio(self):
    #     # self.altdriver.wait_for_element('Button_Play').mobile_tap()
    #     self.skip_tutorial()
    #     self.altdriver.wait_for_element("NPC")
    #     time.sleep(1)

    #     self.go_though_all_steps_without_waiting_for_audio(2,33)

    #     self.driver.save_screenshot('./screenshots/Continue_Button_Tutorial2.png')
    #     self.altdriver.wait_for_element('Button_Continue').mobile_tap()

    text_first_tutorial_npc = {
        0: 'Welcome, welcome to <b><color=\"orange\"><u>Battleground</u></color></b>!',
        1: "You must be tired from your travels - sit, drink, relax and let's play a few rounds!",
        2: 'I\'m Jane - a pleasure!',
        3: 'When you\'re ready, I can show you the ropes. \n\n If you already know how to play, you can just get started',
        4: 'Let\'s dive right in!!',
        5: '<b>Notice</b> your <b><color="orange"><u>Goo Bottles</u></color></b> on the left - that\'s your <b><color="orange"><u>Fuel</u></color></b>',
        6: 'You\'ll need that to power your zombies in order to move them from your \n<b><color="orange"><u>Hand</u></color></b> to the <b><color="orange"><u>Battleground</u></color></b>',
        7: 'Each zombie has a <b><color="orange"><u>Goo Cost</u></color></b>,\n let\xe2\x80\x99s drag your first card into the battleground!',
        8: 'Don\'t worry, I filled up your goo so you can drop a powerful <b><color="orange"><u>General</u></color></b>!',
        9: '<b>Notice</b> the <b><color="orange"><u>zZz</u></color></b> - that means your zombie needs to rest until it activates in the next turn',
        10: 'Wonderful! Now that you\'re done with your turn, you can press the <b><color="orange"><u>End Turn</u></color></b> button, \nright there on the right',
        11: 'Each player, in their turn, will draw zombies to the battleground',
        12: "Let\xe2\x80\x99s wait for your turn and \nsee what we can do",
        13: "Excellent! Let's <b><color=\"orange\"><u>Drag</u></color></b> your zombie onto the enemy zombie so we can attack him!",
        14: "<b>Notice</b> that your <b><color=\"orange\"><u>Attack Value</u></color></b> is deducted from the enemy zombie's <b><color=\"orange\"><u>Defense Value</u></color></b>",
        15: "Well done!! \n You dispatched your first zombie, soon you'll be a veteran <b>Zombie Slayer!!</b>",
        16: "Let\xe2\x80\x99s press <b><color=\"orange\"><u>End Turn</u></color></b>",
        17: "See, your opponent will try to kill your zombies as well, so you have to plan your moves! ",
        18: "The main goal is to beat the enemy <b><color=\"orange\"><u>Overlord</u></color></b>,\nthe first one to do so wins the match, no matter how many zombies are on the battleground!",
        19: "Let's try to attack the enemy Overlord now!",
        20: "Excellent! \nSeems like you're getting the hang of it!",
        21: "Let\xe2\x80\x99s continue",
        22: "Ahh, look - they\xe2\x80\x99re using a <b><color=\"orange\"><u>Heavy</u></color></b> zombie to defend their troops!",
        23: "We'll have to remove it in order \nto attack them again!",
        24: "Damn - you\xe2\x80\x99re out of active zombies on the battleground...",
        25: "Wait - you have some goo and a <b><color=\"orange\"><u>Feral</u></color></b> zombie at the ready in your hand!",
        26: "Feral zombies are a little weaker, but they are super fast - they can attack as soon as they land in the battleground!",
        27: "Let's use one! Drag your feral zombie from your hand into the battleground",
        28: "Now, you can attack your opponent's Overlord immediately!",
        29: "Awesome! Now would be the perfect time to use your <b><color=\"orange\"><u>Overlord Ability</u></color></b>",
        30: "Overlord abilities don\xe2\x80\x99t require Goo\n to use, but they take a few turns\n to get ready at the start of a match\n and after every use.",
        31: "Your Overlord can have two abilities available during a match and can use both of them in a turn if necessary.",
        32: "Time to use one of your <b><color=\"orange\"><u>Overlord Abilities</u></color></b> to attack the enemy Overlord and finish him off!",
        33: "NOICE!! \nYou won your first match!",
        34: 'What did I tell you? \nA bonafide <b>Zombie Slayer</b>, Oh-Yeah!!',
        35: 'Now you\'re ready to start \nyour grand adventure!',
        36: 'Good luck - <b>Zombie Slayer</b>!!'
    }

    text_second_tutorial_npc = {
        0: "Welcome back, Zombie Slayer! \nIt's me Jane, remember? \nTime for advanced play! \nAre you up for it?",
        1: "Looks like you\xe2\x80\x99re in the middle of a match... \n...in a bit of a pickle too.",
        2: "Hmm, one more hit and it's game over. \nThe enemy has one intimidating General zombie in play too, \n...not good at all.",
        3: "YES! Exactly what we needed!",
        4: "Let's drop the big one! <b><color=\"orange\"><u>General</u></color></b> Cherno-bill - and MELT, MELT!",
        5: "To do that you need 11 Goo. \nRight now, you only have 10.",
        6: "Good thing you got Zeptic, \nhe's a <b><color=\"orange\"><u>Goo-Carrier</u></color></b>.",
        7: "Goo-Carriers give you extra goo to spend for this turn only! \nThis effect is called <b><color=\"orange\"><u>Overflow</u></color></b>.",
        8: "Remember, only Goo-Carriers of the same element as your Overlord will work.",
        9: "Overflow is for this turn only, and comes with some setbacks. \nSo... it's a calculated risk!",
        10: "Each goo-carrier has it's own drawback depending on \nit's elemental faction",
        11: "Toxic zombies specialize in dealing extra damage.",
        12: "For example, Toxic Goo-carriers deal damage to your own Overlord.",
        13: "Go ahead, let\xe2\x80\x99s drag Zeptic into play and get Overflow going!",
        14: "Great! ...zz-plan iz unfolding! muhaha",
        15: "We have the goo to drop the big one! \nLet\xe2\x80\x99s show the enemy Overlord how it's done!",
        16: "OH, DID YOU SEE THAT?!?!?",
        17: "Check that out - your Poizoms got +1 Attack and the Destroy ability!",
        18: "This is the <b><color=\"orange\"><u>Ranks System</u></color></b> at work.",
        19: "See - we have ranks in the zombie army, the higher their rank the rarer and more powerful they are!",
        20: "Minions are naturally the weakest, followed by Officers, \nthen Commanders and finally the fearsome Generals!",
        21: "Now, when we have a higher ranking \nzombie in play the lower ranks of its \nelement get buffed. \nThe higher the rank, \nthe stronger the buff.",
        22: "Just remember - each of the 6 elements causes a different effect.",
        23: "For instance, Toxic Generals like Cherno-bill give 3 lower ranking toxic zombies +1 Attack and the Destroy ability.",
        24: "See - now we have a super motivated squad of Poizoms ready for the final blow!",
        25: "Oh, by the way, did I mention that these rank buffs stack?",
        26: "So you can get a crazy combo if you plan it right!",
        27: "Well, what are YOU waiting for?",
        28: "Put this poor excuse of an Overlord out of his misery!",
        29: "AWESOME!! ...Now that\xe2\x80\x99s what I call a proper comeback!",
        30: "Well... That's it! The student has become the master!",
        31: "Now go out there and make me proud - <b><color=\"orange\"><u>Zombie Slayer!!!</u></color></b>",
        32: "Haha, OK...byeee!",
    }

    def check_description(self, step, tutorial):
        if tutorial == 1:
            # self.assertEqual(self.altdriver.wait_for_element('Description/Text').get_component_property('TMPro.TextMeshProUGUI','text','Unity.TextMeshPro'),self.text_first_tutorial_npc.get(step))
            self.wait_for_element_with_tmp_text(
                'Description/Text', self.text_first_tutorial_npc.get(step))
        else:
            self.wait_for_element_with_tmp_text(
                'Description/Text', self.text_second_tutorial_npc.get(step))
            # self.assertEqual(self.altdriver.wait_for_element('Description/Text').get_component_property('TMPro.TextMeshProUGUI','text','Unity.TextMeshPro'),self.text_second_tutorial_npc.get(step))

    def click_next(self, step=0):
        if(step == 11):
            time.sleep(5)
        self.altdriver.find_element('Button_Next').mobile_tap()

    def click_play(self):
        self.altdriver.find_element('Button_Play').mobile_tap()

    def click_end_turn(self):
        self.altdriver.find_element('_1_btn_endturn').mobile_tap()

    def wait_your_turn(self):
        self.altdriver.wait_for_element(
            'EndTurnButton/_1_btn_endturn/EndTurnGlowEffect', timeout=60, interval=1)

    def drag_from_circle_to_circle(self):
        circles = self.altdriver.find_elements("Circle")
        if(int(circles[0].mobileY) < int(circles[1].mobileY)):
            dragFrom = circles[1]
            dragTo = circles[0]
        else:
            dragFrom = circles[0]
            dragTo = circles[1]

        self.driver.swipe(dragFrom.x, dragFrom.mobileY,
                          dragTo.x, dragTo.mobileY, 2000)

    def check_click_next(self, step, tutorial):
        if step == 11 and tutorial == 1:
            self.wait_for_audio_to_finish()
        self.check_description(step, tutorial)
        self.click_next(step=step)

    def check_click_play(self, step, tutorial):
        self.check_description(step, tutorial)
        self.click_play()

    def check_click_end_turn(self, step, tutorial):
        self.check_description(step, tutorial)
        self.click_end_turn()

    def check_drag_from_circles(self, step, tutorial):
        if(step == 28 and tutorial == 1):
            self.wait_for_audio_to_finish()
        self.check_description(step, tutorial)
        self.drag_from_circle_to_circle()
        time.sleep(2)

    def check_wait_turn(self, step, tutorial):
        self.check_description(step, tutorial)
        self.wait_your_turn()

    def wait_turn_check_click_next(self, step, tutorial):
        self.wait_your_turn()
        self.check_click_next(step, tutorial)

    def try_clicking_next_until_description_changes(self, step, tutorial):
        self.check_description(step, tutorial)
        while True:
            try:
                self.check_click_next(step, tutorial)
                time.sleep(1)
            except Exception:
                break

    def try_wait_after_end_turn_until_description_changes_click_next(self, step, tutorial):
        self.wait_your_turn()
        while True:
            try:
                self.check_description(step, tutorial)
                break
                time.sleep(1)
            except Exception:
                time.sleep(1)
        self.check_click_next(step, tutorial)

    def try_swiping_until_description_changes(self, step, tutorial):
        self.check_description(step, tutorial)
        while True:
            try:
                self.check_drag_from_circles(step, tutorial)
                time.sleep(1)
            except Exception:
                break

    def specialAttack(self, step, tutorial):
        self.check_description(step, tutorial)
        # enemy=self.altdriver.wait_for_element('AttackArrowVFX_Object(Clone)/Target_Collider')
        # active_minions=self.altdriver.find_elements('AttackArrowVFX_Object(Clone)/Arrow/Group_RootObjects')

        enemy = self.altdriver.find_element(
            "Opponent/OverlordArea/RegularModel/RegularPosition/Avatar", "CameraBattleground")
        minions = self.altdriver.find_elements(
            "PlayerBoard/BoardCreature(Clone)/Other/ZB_ANM_Walker_ActiveFrame_Green(Clone)/..", "CameraBattleground")

        for minion in minions:
            print(minion.x)
            self.driver.swipe(minion.x, minion.mobileY,
                              enemy.x, enemy.mobileY, 2000)
            time.sleep(1)
        time.sleep(3)

    def wait_for_audio_to_finish(self):
        self.altdriver.wait_for_element('AudioClip TUTORIAL', interval=2)
        self.altdriver.wait_for_element_to_not_be_present(
            'AudioClip TUTORIAL', interval=2)

    steps_first_tutorial = {
        0: check_click_next,
        1: check_click_next,
        2: check_click_next,
        3: check_click_play,
        4: check_click_next,
        5: check_click_next,
        6: check_click_next,
        7: check_click_next,
        8: check_drag_from_circles,
        9: check_click_next,
        10: check_click_end_turn,
        11: check_click_next,
        12: check_wait_turn,
        13: check_drag_from_circles,
        14: check_click_next,
        15: check_click_next,
        16: check_click_end_turn,
        17: check_wait_turn,
        18: check_click_next,
        19: check_drag_from_circles,
        20: check_click_next,
        21: check_click_end_turn,
        22: check_click_next,
        23: check_drag_from_circles,
        24: check_click_next,
        25: check_click_next,
        26: check_click_next,
        27: check_drag_from_circles,
        28: check_drag_from_circles,
        29: check_click_next,
        30: check_click_next,
        31: check_click_next,
        32: check_drag_from_circles,
        33: check_click_next,
        34: check_click_next,
        35: check_click_next,
        36: check_click_next
    }

    steps_second_tutorial = {
        0: check_click_next,
        1: check_wait_turn,
        2: check_click_next,
        3: check_click_next,
        4: check_click_next,
        5: check_click_next,
        6: check_click_next,
        7: check_click_next,
        8: check_click_next,
        9: check_click_next,
        10: check_click_next,
        11: check_click_next,
        12: check_click_next,
        13: check_drag_from_circles,
        14: check_click_next,
        15: check_drag_from_circles,
        16: check_click_next,
        17: check_click_next,
        18: check_click_next,
        19: check_click_next,
        20: check_click_next,
        21: check_click_next,
        22: check_click_next,
        23: check_click_next,
        24: check_click_next,
        25: check_click_next,
        26: check_click_next,
        27: check_click_next,
        28: specialAttack,
        29: check_click_next,
        30: check_click_next,
        31: check_click_next,
        32: check_click_next,

    }

    def skip_tutorial(self):
        self.altdriver.wait_for_element('Button_Skip').mobile_tap()
        self.altdriver.wait_for_element('Button_Yes').mobile_tap()

    def verify_skip_tutorial_is_available(self, tutorial=1):
        button = self.altdriver.wait_for_element('Button_Skip')
        button.mobile_tap()
        if tutorial == 1:
            self.assertEqual(self.altdriver.wait_for_element('QuestionPopup(Clone)/Text_Message').get_component_property(
                'TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro'), 'Do you really want to skip \nBasic Tutorial?')
        else:
            self.assertEqual(self.altdriver.wait_for_element('QuestionPopup(Clone)/Text_Message').get_component_property(
                'TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro'), 'Do you really want to skip \nAdvanced Tutorial?')
        self.altdriver.wait_for_element('Button_Yes')
        self.altdriver.wait_for_element('Button_No').mobile_tap()

    def basic_tutorial_steps(self, step, tutorial):
        if tutorial == 1:
            print('==> basic tutorial - step ' + str(step))
            print('----------------------------------------')
            step_function = self.steps_first_tutorial.get(step)
        else:
            print('==> advanced tutorial - step ' + str(step))
            step_function = self.steps_second_tutorial.get(step)
        print(step_function)
        step_function(self, step, tutorial)

    def go_though_all_steps(self, tutorial, steps):
        for i in range(steps):
            try:
                self.wait_for_audio_to_finish()
                self.verify_skip_tutorial_is_available(tutorial)
                self.basic_tutorial_steps(i, tutorial)
            except Exception:
                print('./screenshots/tutorial-' +
                      str(tutorial)+'-step-'+str(i)+'.png')
                self.driver.save_screenshot(
                    './screenshots/tutorial-'+str(tutorial)+'-step-'+str(i)+'.png')
                raise

    def go_though_all_steps_without_waiting_for_audio(self, tutorial, steps):
        for i in range(steps):
            try:
                time.sleep(1)
                self.verify_skip_tutorial_is_available(tutorial)
                self.basic_tutorial_steps(i, tutorial)
            except Exception:
                print('./screenshots/tutorial-' +
                      str(tutorial)+'-step-'+str(i)+'.png')
                self.driver.save_screenshot(
                    './screenshots/tutorial-'+str(tutorial)+'-step-'+str(i)+'.png')
                raise

    def wait_to_start_tutorial(self):
        self.altdriver.wait_for_current_scene_to_be('GAMEPLAY')
        self.altdriver.wait_for_element("NPC")


if __name__ == '__main__':
    unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))
