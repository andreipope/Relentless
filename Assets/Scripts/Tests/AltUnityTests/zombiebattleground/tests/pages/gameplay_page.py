from base import CZBTests
import time


class Gameplay_Page(CZBTests):
    
    def __init__(self,altdriver,driver):
        self.altdriver=altdriver
        self.driver=driver
        self.altdriver.wait_for_current_scene_to_be('GAMEPLAY')
        self.end_turn_button=self.altdriver.wait_for_element('EndTurnButton/_1_btn_endturn')
        self.player_board=self.altdriver.wait_for_element('Battlefield/PlayerBoard')
        self.opponent_board=self.altdriver.wait_for_element('Battlefield/OpponentBoard')
        self.opponent_face=self.altdriver.wait_for_element('Opponent/OverlordArea/RegularModel/RegularPosition/Avatar/OverlordImage')
        self.player_primary_spell=self.altdriver.wait_for_element('Player/Object_SpellPrimary')
        self.player_secondary_spell=self.altdriver.wait_for_element('Player/Object_SpellSecondary')

    def get_cards_that_are_in_hand(self):
        cards=[]
        cards = self.altdriver.find_elements('CreatureCard(Clone)')
        cards.extend(self.altdriver.find_elements('ItemCard(Clone)'))
        return cards

    def swipe_card_from_hand_to_board(self,card):
        cardGoo = self.altdriver.wait_for_element(
            'id('+str(card.id)+')/GooText')
        self.driver.swipe(int(cardGoo.x), int(cardGoo.mobileY),
                          int(self.player_board.x), int(self.player_board.mobileY), 2000)
    def get_player_board_creatures(self):
        cards=self.altdriver.find_elements('PlayerBoard/BoardCreature(Clone)')
        cards.sort(key=lambda element:float(element.x))
        return cards
    def get_opponent_board_creatures(self):
        cards=self.altdriver.find_elements('OpponentBoard/BoardCreature(Clone)')
        cards.sort(key=lambda element:float(element.x))
        return cards

    def end_turn_and_wait_for_your_turn(self):
        self.end_turn_button.mobile_tap()
        self.altdriver.wait_for_element('id('+str(self.end_turn_button.id)+')/EndTurnGlowEffect', timeout=60)
        time.sleep(5)#sleep is to wait for the card player draws go to player hand
    
    def swipe_card_to_opponent_face(self,card):
        card.mobile_dragToElement(self.opponent_face,2)
    