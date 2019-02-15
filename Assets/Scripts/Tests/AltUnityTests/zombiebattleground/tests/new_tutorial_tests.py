import os
import unittest
from appium import webdriver
from altunityrunner import AltrunUnityDriver
import xmlrunner
import time
from base import CZBTests
import re
import sys
reload(sys)
sys.setdefaultencoding('utf8')


class CZBTutorialTests(CZBTests):

    def setUp(self):
        super(CZBTutorialTests, self).setUp()
    
    def test_first_tutorial(self):
        self.altdriver.wait_for_current_scene_to_be('GAMEPLAY')
        self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()
        self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()
        
        self.altdriver.wait_for_element('TutorialDescriptionTooltip(Clone)')
        time.sleep(2)
        
        board = self.altdriver.wait_for_element('PlayerBoard')
        board.mobile_tap()

        self.altdriver.wait_for_element('CreatureCard(Clone)')
        time.sleep(4)


        card=self.altdriver.wait_for_element('CreatureCard(Clone)')
        cardGoo = self.altdriver.wait_for_element(
            'id('+str(card.id)+')/GooText')
        self.driver.swipe(int(cardGoo.x), int(cardGoo.mobileY),
                          int(board.x), int(board.mobileY), 2000)

        
        self.end_turn_and_wait_for_your_turn()

        playerCard=self.altdriver.wait_for_element('PlayerBoard/BoardCreature(Clone)')
        enemyCard=self.altdriver.wait_for_element('OpponentBoard/BoardCreature(Clone)')

        self.driver.swipe(int(playerCard.x), int(playerCard.mobileY),
                          int(enemyCard.x), int(enemyCard.mobileY), 2000)
       
        self.altdriver.wait_for_element('CreatureCard(Clone)')
        time.sleep(3)
        card=self.altdriver.wait_for_element('CreatureCard(Clone)')
        cardGoo = self.altdriver.wait_for_element(
            'id('+str(card.id)+')/GooText')
        board = self.altdriver.wait_for_element('PlayerBoard')
        self.driver.swipe(int(cardGoo.x), int(cardGoo.mobileY),
                          int(board.x), int(board.mobileY), 2000)

        board.mobile_tap()

        self.end_turn_and_wait_for_your_turn()


        playerCards=self.altdriver.find_elements('PlayerBoard/BoardCreature(Clone)')
        oponentFace=self.altdriver.wait_for_element('Opponent/OverlordArea/RegularModel/RegularPosition/Avatar/OverlordImage')
    
        self.driver.swipe(int(playerCards[1].x), int(playerCards[1].mobileY),
                          int(oponentFace.x), int(oponentFace.mobileY), 2000)
        time.sleep(1)
        self.driver.swipe(int(playerCards[0].x), int(playerCards[0].mobileY),
                          int(oponentFace.x), int(oponentFace.mobileY), 2000)
        time.sleep(2)
        card=self.altdriver.wait_for_element('CreatureCard(Clone)')
        cardGoo = self.altdriver.wait_for_element(
            'id('+str(card.id)+')/GooText')
        self.driver.swipe(int(cardGoo.x), int(cardGoo.mobileY),
                          int(board.x), int(board.mobileY), 2000)

        self.end_turn_and_wait_for_your_turn()


        card=self.altdriver.wait_for_element('ItemCard(Clone)')
        cardGoo = self.altdriver.wait_for_element(
            'id('+str(card.id)+')/GooText')
        board = self.altdriver.wait_for_element('PlayerBoard')
        self.driver.swipe(int(cardGoo.x), int(cardGoo.mobileY),
                          int(board.x), int(board.mobileY), 2000)
        time.sleep(1)
        oponentFace.mobile_tap()
        self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()

    def end_turn_and_wait_for_your_turn(self):
        self.altdriver.wait_for_element('EndTurnButton/_1_btn_endturn').mobile_tap()
        self.altdriver.wait_for_element(
            'EndTurnButton/_1_btn_endturn/EndTurnGlowEffect', timeout=60)
        time.sleep(4)


             







        
         


   

if __name__ == '__main__':
    unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))
