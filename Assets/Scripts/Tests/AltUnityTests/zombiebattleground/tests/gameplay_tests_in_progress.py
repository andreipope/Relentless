import sys
import unittest

from appium import webdriver
from altunityrunner import AltElement, AltrunUnityDriver, AltUnityException
from base import CZBTests
import time
import xmlrunner


reload(sys)
sys.setdefaultencoding('utf8')

class CZBPlayTests(CZBTests):

    def setUp(self):
        super(CZBPlayTests, self).setUp()
        self.altdriver.wait_for_element('PressAnyText').mobile_tap()
        self.altdriver.wait_for_element('InputField_Beta').set_component_property('UnityEngine.UI.InputField','text', self.tester_key)
        self.altdriver.find_element('Button_Beta').tap()
        self.altdriver.wait_for_element('TermsPopup(Clone)/Toggle').tap()
        self.altdriver.wait_for_element('Button_GotIt').tap()
        self.driver.save_screenshot('./screenshots/login-before-credits-tests.png')


    def test_start_match_versus_ai(self):
        self.skip_both_tutorials()
        time.sleep(5)
        self.altdriver.wait_for_element('Button_Battle').mobile_tap()
        self.altdriver.wait_for_current_scene_to_be('GAMEPLAY')
        self.altdriver.wait_for_element('Button_Keep').mobile_tap()
        while self.game_not_ended():
            self.your_turn()


    def your_turn(self):
        self.altdriver.wait_for_element('EndTurnButton/_1_btn_endturn/EndTurnGlowEffect',timeout=60)
        self.check_player_goo_bottle_to_be_refilled()
        self.altdriver.wait_for_element('EndTurnButton/_1_btn_endturn').mobile_tap()
        time.sleep(1)
        self.check_opponent_goo_bottle_to_be_refilled()
        
    def get_player_defence(self):
        return int(self.altdriver.wait_for_element('Player/OverlordArea/RegularModel/RegularPosition/Avatar/Deffence/DefenceText').get_component_property('TMPro.TextMeshPro','text','Unity.TextMeshPro'))
    
    def get_opponent_defence(self):
        return int(self.altdriver.wait_for_element('Opponent/OverlordArea/RegularModel/RegularPosition/Avatar/Deffence/DefenceText').get_component_property('TMPro.TextMeshPro','text','Unity.TextMeshPro'))


    def check_player_goo_bottle_to_be_refilled(self):
        number_of_bottles=len(self.altdriver.find_elements('PlayerManaBar/BottleGoo'))
        number_of_goo=len(self.altdriver.find_elements('PlayerManaBar/BottleGoo/New Sprite Mask/goo'))
        self.assertEqual(number_of_bottles,number_of_goo)
    
    def check_opponent_goo_bottle_to_be_refilled(self):
        number_of_bottles=len(self.altdriver.find_elements('OpponentManaBar/BottleGoo'))
        number_of_goo=len(self.altdriver.find_elements('OpponentManaBar/BottleGoo/New Sprite Mask/goo'))
        self.assertEqual(number_of_bottles,number_of_goo)
        
    def game_not_ended(self):
        while True:
            try:
                self.altdriver.find_element('YouLosePopup(Clone)')
                return False
            except Exception:
                print('YouLosePopup not found')
            try:
                self.altdriver.find_element('YouWinPopup(Clone)')
                return False
            except Exception:
                print('YouWinPopup not found')
            try:
                self.altdriver.find_element('EndTurnButton/_1_btn_endturn/EndTurnGlowEffect')
                return True
            except Exception:
                print('YouWinPopup not found')
            time.sleep(3)
            

    

if __name__ == '__main__':
    unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))